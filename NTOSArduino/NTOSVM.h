#pragma once

#include <Arduino.h>

// ============================================================
// NTOS VM configuration
// ============================================================

#ifndef NTOS_BYTECODE_SIZE
#define NTOS_BYTECODE_SIZE 4096
#endif

#ifndef NTOS_MEMORY_SIZE
#define NTOS_MEMORY_SIZE 2048
#endif

#ifndef NTOS_STACK_SIZE
#define NTOS_STACK_SIZE 64
#endif

#ifndef NTOS_CALL_STACK_SIZE
#define NTOS_CALL_STACK_SIZE 32
#endif

#ifndef NTOS_DEBUG
#define NTOS_DEBUG 1
#endif


// ============================================================
// Stack
// ============================================================

enum class StackValueType : uint8_t {
  None = 0,
  Int,
  Float,
  Byte,
  Bool,
  String
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




public:

  // ========================================================
  // Program
  // ========================================================

  static bool LoadBytecode(
    const uint8_t* bytecode,
    uint16_t size) {
    if (bytecode == nullptr)
      return false;

    if (size > NTOS_BYTECODE_SIZE)
      return false;

    memcpy(
      _bytecode,
      bytecode,
      size);

    _bytecodeSize = size;

    ResetExecution();

    return true;
  }

  static bool LoadBytecodeFromFlash(
    const uint8_t* bytecode,
    uint16_t size) {

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


  // ========================================================
  // Execution state
  // ========================================================

  static void ResetExecution() {
    _ip = 0;
    _sp = 0;
    _callSp = 0;
    _running = false;
  }

  static void Start() {
    _ip = 0;
    _sp = 0;
    _callSp = 0;
    _running = true;
  }

  static void Stop() {
    _running = false;
  }


  static void ClearMemory() {
    memset(
      _memory,
      0,
      sizeof(_memory));
  }


  // ========================================================
  // Execute
  // ========================================================

  static void Update() {

    if (!_running)
      return;

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
          uint16_t address =
            ReadUInt16();

          // TODO:
          // Load string from variable memory.

          (void)address;

          break;
        }


      case 0x0C:  // StoreStr
        {
          uint16_t address =
            ReadUInt16();

          StackValue value =
            Pop();

          // TODO:
          // Store string into variable memory.

          (void)address;
          (void)value;

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


      case 0x83:  // CallFunction
        {
          uint16_t functionIndex = ReadUInt16();
          uint8_t argumentCount = ReadByte();

          ExecuteSystemFunction(
            functionIndex,
            argumentCount);

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
        // Delay
        // ------------------------------------------------

      case 0x85:  // Delay
        {
          float seconds =
            ReadFloat();

          delay(
            static_cast<unsigned long>(
              seconds * 1000.0f));

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

  static void Push(
    StackValue value) {
    if (_sp >= NTOS_STACK_SIZE)
      return;

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
    if (_ip >= _bytecodeSize)
      return 0;

    return _bytecode[_ip++];
  }


  static uint16_t ReadUInt16() {
    if (_ip + 1 >= _bytecodeSize)
      return 0;

    uint16_t value =
      static_cast<uint16_t>(
        _bytecode[_ip])
      | (static_cast<uint16_t>(
           _bytecode[_ip + 1])
         << 8);

    _ip += 2;

    return value;
  }


  static int32_t ReadInt32() {
    if (_ip + 3 >= _bytecodeSize)
      return 0;

    int32_t value =
      static_cast<int32_t>(
        _bytecode[_ip])
      | (static_cast<int32_t>(
           _bytecode[_ip + 1])
         << 8)
      | (static_cast<int32_t>(
           _bytecode[_ip + 2])
         << 16)
      | (static_cast<int32_t>(
           _bytecode[_ip + 3])
         << 24);

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

    if (address < 1000)
      Serial.print('0');
    if (address < 100)
      Serial.print('0');
    if (address < 10)
      Serial.print('0');

    Serial.print(address);

    Serial.print(F(" OPCODE=0x"));

    if (opcode < 0x10)
      Serial.print('0');

    Serial.println(opcode, HEX);
#endif
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

  static void ExecuteSystemFunction(
    uint16_t functionIndex,
    uint8_t argumentCount) {
    switch (functionIndex) {
      case 0:  // cls
        {
          tft.fillScreenBlack();
          break;
        }

      case 1:  // fillCircle
        {
          uint8_t color = Pop();
          uint32_t radius = Pop();
          uint32_t y = Pop();
          uint32_t x = Pop();

          UI::fillCircle(
            (int16_t)x,
            (int16_t)y,
            (int16_t)radius,
            Color332To565(color));

          break;
        }

      default:
        Serial.print("[NTOS] Unknown system function: ");
        Serial.println(functionIndex);
        break;
    }
  }
};
