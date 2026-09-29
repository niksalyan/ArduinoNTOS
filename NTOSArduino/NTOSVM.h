#pragma once

#define SOFTWARE_SPI_FOR_SD
#include <SoftSD.h>
#include <Arduino.h>
#include "UI.h"
#include "Navigation.h"
#include "Storage.h"

// ============================================================
// NTOS VM configuration
// ============================================================
#define NTOS_BYTECODE_SIZE 2560
#define NTOS_MEMORY_SIZE 1536
#define NTOS_STACK_SIZE 64
#define NTOS_CALL_STACK_SIZE 32
#define NTOS_DEBUG 0


// ============================================================
// Stack
// ============================================================

enum class StackValueType : uint8_t {
  None = 0,
  Int,
  Float,
  Byte,
  Bool,
  String,
  MemoryString
};


struct StackValue {
  StackValueType type;
  uint32_t value;
};


// ============================================================
// NTOS VM
// ============================================================

class NTOSVM {
private:

  inline static uint8_t _bytecode[NTOS_BYTECODE_SIZE] = {};
  inline static uint16_t _bytecodeSize = 0;

  inline static uint8_t _memory[NTOS_MEMORY_SIZE] = {};
  inline static uint16_t _ip = 0;

  inline static StackValue _stack[NTOS_STACK_SIZE] = {};
  inline static uint8_t _sp = 0;

  inline static uint16_t _callStack[NTOS_CALL_STACK_SIZE] = {};
  inline static uint8_t _callSp = 0;

  inline static bool _running = false;
  inline static int32_t _delay = 0;
  inline static char* appName = "";



public:

  inline static bool initialized = false;
  inline static char currentKey = 0;

  // ========================================================
  // Program
  // ========================================================
  static bool LoadBytecodeFromFlash(
    const uint8_t* bytecode,
    uint16_t size) {

    ClearBytecode();

    if (bytecode == nullptr)
      return false;

    if (size > NTOS_BYTECODE_SIZE)
      return false;

    memcpy_P(
      _bytecode,
      bytecode,
      size);

    _bytecodeSize = size;

    ResetExecution();

    return true;
  }

  static bool LoadBytecodeFromFile(File& file, char* name) {
    appName = name;
    ClearBytecode();

    if (!file)
      return false;

    uint32_t size = file.size();

    if (size == 0 || size > NTOS_BYTECODE_SIZE)
      return false;

    size_t bytesRead = file.read(
      _bytecode,
      size);

    _bytecodeSize = bytesRead;

    ResetExecution();

    return true;
  }

#if NTOS_DEBUG
  static void DumpBytecode() {
    Serial.println(F("HEX: "));

    for (uint16_t i = 0; i < _bytecodeSize; i++) {
      if (_bytecode[i] < 0x10)
        Serial.print('0');

      Serial.print(_bytecode[i], HEX);
    }

    Serial.println();
  }
#endif

  // ========================================================
  // Execution state
  // ========================================================

  static void ResetExecution() {
    _ip = 0;
    _sp = 0;
    _callSp = 0;
    _running = false;
    _delay = 0;
  }

  static void Start() {
    _ip = 0;
    _sp = 0;
    _callSp = 0;
    _running = true;
    _delay = 0;
#if NTOS_DEBUG
    DumpBytecode();
#endif
  }

  static void Stop() {
    _running = false;
    _delay = 0;
  }


  static void ClearMemory() {
    memset(
      _memory,
      0,
      sizeof(_memory));
  }

  static void ClearBytecode() {
    memset(
      _bytecode,
      0,
      sizeof(_bytecode));
    _bytecodeSize = 0;
  }


  // ========================================================
  // Execute
  // ========================================================

  static bool IsRunning() {
    return _running;
  }

  static void Update() {

    if (!_running) {
      return;
    }

    if (_delay > 0) {
      delay(1);
      _delay--;
      return;
    }

    if (_ip >= _bytecodeSize) {
      _running = false;
      return;
    }


    uint16_t instructionAddress = _ip;

    uint8_t rawOpcode = ReadByte();

    DebugInstruction(
      instructionAddress,
      rawOpcode);

    switch (rawOpcode) {
        // ------------------------------------------------
        // Program
        // ------------------------------------------------

      case 0x00:  // End
        {
          Stop();
          Navigation::MainView();
          return;
        }


        // ------------------------------------------------
        // Constants
        // ------------------------------------------------

      case 0x01:  // PushInt
        {
          int32_t value =
            ReadInt32();

          Push({ StackValueType::Int,
                 static_cast<uint32_t>(value) });

          break;
        }


      case 0x02:  // PushFloat
        {
          float value =
            ReadFloat();

          uint32_t bits;

          memcpy(
            &bits,
            &value,
            sizeof(bits));

          Push({ StackValueType::Float,
                 bits });

          break;
        }


      case 0x03:  // PushStr
        {
          // _ip points to the first character.

          uint16_t stringOffset =
            _ip;

          Push({ StackValueType::String,
                 stringOffset });

          // Skip null-terminated string.

          while (_ip < _bytecodeSize) {
            if (_bytecode[_ip++] == 0)
              break;
          }

          break;
        }


      case 0x04:  // PushByte
        {
          uint8_t value =
            ReadByte();

          Push({ StackValueType::Byte,
                 value });

          break;
        }


        // ------------------------------------------------
        // Variables
        // ------------------------------------------------

      case 0x05:  // LoadInt
        {
          uint16_t address =
            ReadUInt16();

          int32_t value =
            GetInt(address);

          Push({ StackValueType::Int,
                 static_cast<uint32_t>(value) });

          break;
        }


      case 0x06:  // StoreInt
        {
          uint16_t address =
            ReadUInt16();

          StackValue value =
            Pop();

          SetInt(
            address,
            static_cast<int32_t>(
              value.value));

          break;
        }


      case 0x07:  // LoadFloat
        {
          uint16_t address =
            ReadUInt16();

          float value =
            GetFloat(address);

          uint32_t bits;

          memcpy(
            &bits,
            &value,
            sizeof(bits));

          Push({ StackValueType::Float,
                 bits });

          break;
        }


      case 0x08:  // StoreFloat
        {
          uint16_t address =
            ReadUInt16();

          StackValue value =
            Pop();

          float number;

          uint32_t bits =
            value.value;

          memcpy(
            &number,
            &bits,
            sizeof(number));

          SetFloat(
            address,
            number);

          break;
        }


      case 0x09:  // LoadByte
        {
          uint16_t address =
            ReadUInt16();

          Push({ StackValueType::Byte,
                 GetByte(address) });

          break;
        }


      case 0x0A:  // StoreByte
        {
          uint16_t address =
            ReadUInt16();

          StackValue value =
            Pop();

          SetByte(
            address,
            static_cast<uint8_t>(
              value.value));

          break;
        }


      case 0x0B:  // LoadStr
        {
          uint16_t address = ReadUInt16();

          Push({ StackValueType::MemoryString,
                 address });

          break;
        }

      case 0x0C:  // StoreStr
        {
          uint16_t address = ReadUInt16();

          StackValue value = Pop();

          StoreString(address, value);

          break;
        }
        // ------------------------------------------------
        // Arithmetic
        // ------------------------------------------------

      case 0x20:  // Add
        {
          BinaryNumeric('+');
          break;
        }


      case 0x21:  // Subtract
        {
          BinaryNumeric('-');
          break;
        }


      case 0x22:  // Multiply
        {
          BinaryNumeric('*');
          break;
        }


      case 0x23:  // Divide
        {
          BinaryNumeric('/');
          break;
        }


      case 0x24:  // Modulo
        {
          BinaryNumeric('%');
          break;
        }


        // ------------------------------------------------
        // Stack
        // ------------------------------------------------

      case 0x25:  // Pop
        {
          Pop();
          break;
        }


        // ------------------------------------------------
        // Comparison
        // ------------------------------------------------

      case 0x26:  // Equal
        {
          StackValue right = Pop();
          StackValue left = Pop();

          Push({ StackValueType::Bool,
                 AreEqual(left, right)
                   ? 1
                   : 0 });

          break;
        }


      case 0x27:  // NotEqual
        {
          StackValue right = Pop();
          StackValue left = Pop();

          Push({ StackValueType::Bool,
                 AreEqual(left, right)
                   ? 0
                   : 1 });

          break;
        }


      case 0x28:  // Less
        {
          Compare('<');
          break;
        }


      case 0x29:  // Greater
        {
          Compare('>');
          break;
        }


      case 0x2A:  // LessEqual
        {
          Compare('L');
          break;
        }


      case 0x2B:  // GreaterEqual
        {
          Compare('G');
          break;
        }


        // ------------------------------------------------
        // Boolean
        // ------------------------------------------------

      case 0x2C:  // And
        {
          StackValue right = Pop();
          StackValue left = Pop();

          Push({ StackValueType::Bool,
                 (left.value && right.value)
                   ? 1
                   : 0 });

          break;
        }


      case 0x2D:  // Or
        {
          StackValue right = Pop();
          StackValue left = Pop();

          Push({ StackValueType::Bool,
                 (left.value || right.value)
                   ? 1
                   : 0 });

          break;
        }


      case 0x2E:  // Not
        {
          StackValue value =
            Pop();

          Push({ StackValueType::Bool,
                 value.value
                   ? 0
                   : 1 });

          break;
        }

      case 0x2F:
        {
          StackValue address = Pop();

          int32_t value =
            GetInt(
              static_cast<uint16_t>(
                address.value));

          Push({ StackValueType::Int,
                 static_cast<uint32_t>(value) });

          break;
        }

      case 0x30:
        {
          StackValue value = Pop();
          StackValue address = Pop();

          SetInt(
            static_cast<uint16_t>(
              address.value),
            static_cast<int32_t>(
              value.value));

          break;
        }

      case 0x31:  // LoadIndirectStr
        {
          StackValue address = Pop();

          Push({ StackValueType::MemoryString,
                 address.value });

          break;
        }

      case 0x32:  // StoreIndirectStr
        {
          StackValue value = Pop();
          StackValue address = Pop();

          StoreString(
            static_cast<uint16_t>(address.value),
            value);

          break;
        }

        // ------------------------------------------------
        // Flow control
        // ------------------------------------------------

      case 0x80:  // Jump
        {
          _ip =
            ReadUInt16();

          break;
        }


      case 0x81:  // JumpIfFalse
        {
          uint16_t target =
            ReadUInt16();

          StackValue condition =
            Pop();

          if (!condition.value)
            _ip = target;

          break;
        }


      case 0x82:  // CallSubroutine
        {
          uint16_t target =
            ReadUInt16();

          if (_callSp >= NTOS_CALL_STACK_SIZE)
            return;

          _callStack[_callSp++] =
            _ip;

          _ip = target;

          break;
        }


      case 0x83:  // Call Function
        {
          uint16_t functionIndex = ReadUInt16();
          uint8_t argumentCount = ReadByte();

          bool hasReturnValue =
            ExecuteSystemFunction(
              functionIndex,
              argumentCount);

          if (!hasReturnValue) {
            Push({ StackValueType::Int,
                   0 });
          }

          break;
        }


        // ------------------------------------------------
        // Subroutine return
        // ------------------------------------------------

      case 0x84:  // Return
        {
          if (_callSp == 0)
            return;

          _ip =
            _callStack[--_callSp];

          break;
        }
        // ------------------------------------------------
        // JumpIfInitialized
        // ------------------------------------------------

      case 0x85:  // JumpIfInitialized
        {
          uint16_t target =
            ReadUInt16();

          if (initialized) {
            _ip = target;
          }

          break;
        }


        // ------------------------------------------------
        // Debug / checkpoint
        // ------------------------------------------------

      case 0xFF:  // Checkpoint
        {
          // Reserved for debugging.
          break;
        }


        // ------------------------------------------------
        // Unknown opcode
        // ------------------------------------------------

      default:
        {
          // Invalid opcode.
          return;
        }
    }
  }


private:

  // ========================================================
  // Stack
  // ========================================================

  static void Push(StackValue value) {
    if (_sp >= NTOS_STACK_SIZE) {
      return;
    }
    _stack[_sp++] = value;
  }


  static StackValue Pop() {
    if (_sp == 0) {
      return {
        StackValueType::None,
        0
      };
    }

    return _stack[--_sp];
  }


  // ========================================================
  // Bytecode readers
  // ========================================================

  static uint8_t ReadByte() {
    uint16_t before = _ip;

    if (_ip >= _bytecodeSize) {
      return 0;
    }

    return _bytecode[_ip++];
  }


  static uint16_t ReadUInt16() {
    uint16_t before = _ip;

    if (_ip + 1 >= _bytecodeSize) {
      return 0;
    }

    uint16_t value =
      static_cast<uint16_t>(_bytecode[_ip]) | (static_cast<uint16_t>(_bytecode[_ip + 1]) << 8);

    _ip += 2;

    return value;
  }


  static int32_t ReadInt32() {
    uint16_t before = _ip;

    if (_ip + 3 >= _bytecodeSize) {
      return 0;
    }

    int32_t value =
      static_cast<int32_t>(_bytecode[_ip]) | (static_cast<int32_t>(_bytecode[_ip + 1]) << 8) | (static_cast<int32_t>(_bytecode[_ip + 2]) << 16) | (static_cast<int32_t>(_bytecode[_ip + 3]) << 24);

    _ip += 4;
    return value;
  }


  static float ReadFloat() {
    uint32_t bits =
      static_cast<uint32_t>(
        ReadInt32());

    float value;

    memcpy(
      &value,
      &bits,
      sizeof(value));

    return value;
  }


  // ========================================================
  // Memory
  // ========================================================

  static void SetInt(
    uint16_t address,
    int32_t value) {
    if (address + 4 > NTOS_MEMORY_SIZE)
      return;

    memcpy(
      &_memory[address],
      &value,
      sizeof(value));
  }


  static int32_t GetInt(
    uint16_t address) {
    if (address + 4 > NTOS_MEMORY_SIZE)
      return 0;

    int32_t value;

    memcpy(
      &value,
      &_memory[address],
      sizeof(value));

    return value;
  }


  static void SetFloat(
    uint16_t address,
    float value) {
    if (address + 4 > NTOS_MEMORY_SIZE)
      return;

    memcpy(
      &_memory[address],
      &value,
      sizeof(value));
  }


  static float GetFloat(
    uint16_t address) {
    if (address + 4 > NTOS_MEMORY_SIZE)
      return 0.0f;

    float value;

    memcpy(
      &value,
      &_memory[address],
      sizeof(value));

    return value;
  }


  static void SetByte(
    uint16_t address,
    uint8_t value) {
    if (address >= NTOS_MEMORY_SIZE)
      return;

    _memory[address] = value;
  }


  static uint8_t GetByte(
    uint16_t address) {
    if (address >= NTOS_MEMORY_SIZE)
      return 0;

    return _memory[address];
  }


  // ========================================================
  // Arithmetic
  // ========================================================

  static void BinaryNumeric(
    char operation) {
    StackValue right = Pop();
    StackValue left = Pop();

    int32_t a =
      static_cast<int32_t>(
        left.value);

    int32_t b =
      static_cast<int32_t>(
        right.value);

    int32_t result = 0;

    switch (operation) {
      case '+':
        result = a + b;
        break;

      case '-':
        result = a - b;
        break;

      case '*':
        result = a * b;
        break;

      case '/':
        if (b != 0)
          result = a / b;
        break;

      case '%':
        if (b != 0)
          result = a % b;
        break;
    }

    Push({ StackValueType::Int,
           static_cast<uint32_t>(result) });
  }


  // ========================================================
  // Comparison
  // ========================================================

  static void Compare(
    char operation) {
    StackValue right = Pop();
    StackValue left = Pop();

    int32_t a =
      static_cast<int32_t>(
        left.value);

    int32_t b =
      static_cast<int32_t>(
        right.value);

    bool result = false;

    switch (operation) {
      case '<':
        result = a < b;
        break;

      case '>':
        result = a > b;
        break;

      case 'L':
        result = a <= b;
        break;

      case 'G':
        result = a >= b;
        break;
    }

    Push({ StackValueType::Bool,
           result ? 1 : 0 });
  }


  // ========================================================
  // Equality
  // ========================================================

  static bool AreEqual(
    const StackValue& left,
    const StackValue& right) {
    if (left.type != right.type)
      return false;

    return left.value == right.value;
  }

  static void DebugInstruction(
    uint16_t address,
    uint8_t opcode) {

#if NTOS_DEBUG

    Serial.print(F("[NTOS] IP="));
    Serial.print(address);

    Serial.print(F(" OPCODE=0x"));
    if (opcode < 0x10) Serial.print('0');
    Serial.print(opcode, HEX);

    Serial.print(F(" SP="));
    Serial.print(_sp);

    Serial.print(F(" BYTES="));

    uint16_t end = address + 12;
    if (end > _bytecodeSize)
      end = _bytecodeSize;

    for (uint16_t i = address; i < end; i++) {
      if (_bytecode[i] < 0x10)
        Serial.print('0');

      Serial.print(_bytecode[i], HEX);
      Serial.print(' ');
    }

    Serial.println();

#endif
  }

  static void StoreString(
    uint16_t address,
    const StackValue& value) {

    const char* source =
      GetStringPointer(value);

    while (*source != '\0') {
      if (address >= NTOS_MEMORY_SIZE - 1)
        break;

      _memory[address++] =
        static_cast<uint8_t>(*source++);
    }

    _memory[address] = 0;
  }

  static const char* GetStringPointer(
    const StackValue& value) {
    if (value.type == StackValueType::String) {
      return reinterpret_cast<const char*>(
        &_bytecode[value.value]);
    }

    if (value.type == StackValueType::MemoryString) {
      return reinterpret_cast<const char*>(
        &_memory[value.value]);
    }

    return "";
  }

  static uint16_t Color332To565(uint8_t color) {
    uint8_t r = (color >> 5) & 0x07;
    uint8_t g = (color >> 2) & 0x07;
    uint8_t b = color & 0x03;

    // Expand RGB332 to RGB888
    r = (r * 255) / 7;
    g = (g * 255) / 7;
    b = (b * 255) / 3;

    return tft.color565(r, g, b);
  }

  static void print(StackValue textValue, int position = 0) {
    int16_t bx;
    int16_t by;
    uint16_t w;
    uint16_t h;
    switch (textValue.type) {

      case StackValueType::String:
      case StackValueType::MemoryString:
        {
          const char* text =
            GetStringPointer(textValue);

          if (position > 0) {
            tft.getTextBounds(
              text,
              0,
              0,
              &bx,
              &by,
              &w,
              &h);

            tft.setCursor(
              tft.getCursorX() - w / 2 * position,
              tft.getCursorY());
          }

          tft.print(text);
          break;
        }
      case StackValueType::Int:
        {
          int32_t value =
            (int32_t)textValue.value;

          tft.print(value);
          break;
        }

      case StackValueType::Float:
        {
          float value;

          uint32_t bits =
            textValue.value;

          memcpy(
            &value,
            &bits,
            sizeof(value));

          tft.print(value);
          break;
        }

      case StackValueType::Byte:
        {
          uint8_t value =
            (uint8_t)textValue.value;

          tft.print(value);
          break;
        }

      case StackValueType::Bool:
        {
          bool value =
            textValue.value != 0;

          tft.print(value ? "true" : "false");
          break;
        }

      default:
        break;
    }
  }

  static bool ExecuteSystemFunction(
    uint16_t functionIndex,
    uint8_t argumentCount) {
    switch (functionIndex) {
      case 0:  // debug
        {
          uint16_t stringOffset = (uint16_t)Pop().value;

          const char* text =
            reinterpret_cast<const char*>(
              &_bytecode[stringOffset]);

          Serial.print("->");
          Serial.print(text);
          break;
        }
      case 1:  // load() /// The load name need to prefix AppName/<text>.ntos // What is the best way to do it ?
        {
          uint16_t stringOffset = (uint16_t)Pop().value;

          const char* text =
            reinterpret_cast<const char*>(
              &_bytecode[stringOffset]);

          Stop();
          ResetExecution();

          File file;

          if (!Storage::openAppFile(appName, text, "ntx", file)) {
            return;
          }

          bool loaded = LoadBytecodeFromFile(file, appName);

          file.close();

          if (!loaded) {
            return;
          }
          NTOSVM::initialized = true;
          NTOSVM::Start();


          break;
        }

      case 2:  // exit
        {
          Stop();
          Navigation::MainView();
          break;
        }

      case 3:  // delay
        {
          _delay = (int32_t)Pop().value;
          break;
        }
      case 4:  // getKey()
        {
          uint8_t key = currentKey;
          currentKey = 0;

          Push({ StackValueType::Byte,
                 key });

          return true;
        }

      case 9:  // cls
        {
          tft.setTextSize(2);
          tft.setCursor(0, 0);
          tft.fillScreenBlack();
          break;
        }

      case 10:  // drawBox
        {
          uint8_t color = (uint8_t)Pop().value;
          uint32_t h = Pop().value;
          uint32_t w = Pop().value;
          uint32_t y = Pop().value;
          uint32_t x = Pop().value;

          tft.drawRect(x, y, w, h, Color332To565(color));

          break;
        }

      case 11:  // fillBox
        {
          uint8_t color = (uint8_t)Pop().value;
          uint32_t h = Pop().value;
          uint32_t w = Pop().value;
          uint32_t y = Pop().value;
          uint32_t x = Pop().value;
          if (color == 0) {
            tft.fastFillRectBlack(x, y, w, h);
          } else {
            tft.fastFillRect(x, y, w, h, Color332To565(color));
          }
          break;
        }

      case 12:  // pixel
        {
          uint8_t color = (uint8_t)Pop().value;
          uint32_t y = Pop().value;
          uint32_t x = Pop().value;

          tft.drawPixel(x, y, Color332To565(color));

          break;
        }

      case 13:  // line
        {
          uint8_t color = (uint8_t)Pop().value;
          uint32_t y2 = Pop().value;
          uint32_t x2 = Pop().value;
          uint32_t y1 = Pop().value;
          uint32_t x1 = Pop().value;

          tft.drawLine(x1, y1, x2, y2, Color332To565(color));

          break;
        }

      case 14:  // drawCircle
        {
          uint8_t color = (uint8_t)Pop().value;
          uint32_t r = Pop().value;
          uint32_t y = Pop().value;
          uint32_t x = Pop().value;

          tft.drawCircle(x, y, r, Color332To565(color));

          break;
        }

      case 15:  // fillCircle
        {
          uint8_t color = (uint8_t)Pop().value;
          uint32_t r = Pop().value;
          uint32_t y = Pop().value;
          uint32_t x = Pop().value;

          tft.fillCircle(x, y, r, Color332To565(color));

          break;
        }
      case 20:  // cursor
        {
          int16_t sizeValue = (int16_t)Pop().value;
          int16_t yValue = (int16_t)Pop().value;
          int16_t xValue = (int16_t)Pop().value;

          tft.setTextSize(sizeValue);
          tft.setCursor(xValue, yValue);
          break;
        }
      case 21:  // print
        {
          uint8_t color = (uint8_t)Pop().value;
          StackValue textValue = Pop();
          tft.setTextColor(
            Color332To565(color));
          print(textValue);
          break;
        }

      case 22:  // printCentered
        {
          uint8_t color = (uint8_t)Pop().value;
          StackValue textValue = Pop();
          tft.setTextColor(
            Color332To565(color));
          print(textValue, 1);
          break;
        }
      case 23:  // printRight
        {
          uint8_t color = (uint8_t)Pop().value;
          StackValue textValue = Pop();
          tft.setTextColor(
            Color332To565(color));
          print(textValue, 2);
          break;
        }

      case 30:  // dialog
        {
          const char* text = GetStringPointer(Pop());
          UI::dialog(text);
          tft.setTextSize(2);
          tft.setCursor(0, 26);

          break;
        }

      default:
        Serial.print("[NTOS] Unknown system function: ");
        Serial.println(functionIndex);

        for (uint8_t i = 0; i < argumentCount; i++) {
          Pop();
        }
        break;
    }
    return false;
  }
};
