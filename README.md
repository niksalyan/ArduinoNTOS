# NTOS

### A tiny operating system for Arduino, with an Arduino JavaScript programming language.

**NTOS** is an experimental operating system designed for Arduino Mega with an Adafruit 3.5" TFT Shield and SD card.

It includes its own **programming language, compiler, bytecode format, virtual machine, runtime, application system, graphics APIs, and development environment**.

The NTOS programming language uses familiar **JavaScript syntax** and is parsed by **Acornima**, a real JavaScript parser. The resulting JavaScript AST is compiled into custom NTOS bytecode and executed by the NTOS Virtual Machine on the Arduino.

> **Arduino JavaScript → JavaScript AST → NTOS Compiler → NTOS Bytecode → NTOS VM → Arduino**

The Arduino does **not** run a JavaScript interpreter.

JavaScript parsing happens during compilation on the development machine. The Arduino receives and executes the resulting NTOS bytecode.

The goal isn't to build the smallest Arduino project.

The goal is to understand how an operating system works by building one from the ground up.

---

# 🚀 The Story

NTOS started with a simple idea:

> **Can an Arduino run applications directly from an SD card?**

The first idea was simply to store applications on an SD card and load them when needed.

But that immediately created another problem:

**How does the Arduino execute an application stored on the SD card?**

Instead of putting an interpreter on the Arduino, NTOS uses a custom bytecode format and a virtual machine.

Then came the compiler.

Then variables.

Then functions.

Then memory management.

Then graphics.

Then application loading.

Then resources.

Then a development environment.

At some point, the project stopped being an experiment for running programs from an SD card.

It had become an operating system.

And that's how **NTOS** was born.

---

# 🧠 What Is NTOS?

NTOS is essentially a small software stack running on an Arduino:

```text
┌──────────────────────────────┐
│        NTOS Applications     │
│          *.ntx / *.nti       │
├──────────────────────────────┤
│          NTOS Runtime        │
├──────────────────────────────┤
│       NTOS Virtual Machine   │
├──────────────────────────────┤
│       Arduino Hardware       │
├──────────────────────────────┤
│ TFT Display │ SD Card │ I/O  │
└──────────────────────────────┘
```

Applications are written using NTOS source code and compiled into NTOS bytecode.

The bytecode is stored on the SD card.

The Arduino loads the bytecode and executes it through the NTOS Virtual Machine.

The firmware provides the runtime and hardware interface, while applications remain separate.

---

# 🏗️ Architecture

The complete development pipeline looks like this:

```text
                         Development PC
                               │
                               ▼
                    ┌─────────────────────┐
                    │      NTOS Dev       │
                    │                     │
                    │ Source Code         │
                    │ Resources           │
                    │ Build Tools         │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │      Acornima       │
                    │  JavaScript Parser  │
                    └──────────┬──────────┘
                               │
                               ▼
                         JavaScript AST
                               │
                               ▼
                    ┌─────────────────────┐
                    │    NTOS Compiler    │
                    │      AST Visitor    │
                    └──────────┬──────────┘
                               │
                               ▼
                       NTOS Instructions
                               │
                               ▼
                    ┌─────────────────────┐
                    │   Bytecode Builder  │
                    └──────────┬──────────┘
                               │
                               ▼
                         NTOS Bytecode
                             *.ntx
                               │
                               ▼
                          ┌─────────┐
                          │ SD Card │
                          └────┬────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │       NTOS VM       │
                    │       Arduino       │
                    └──────────┬──────────┘
                               │
                  ┌────────────┼────────────┐
                  ▼            ▼            ▼
               Graphics      Input       Storage
                  │            │            │
                  └────────────┼────────────┘
                               ▼
                         TFT Display
```

There are two very different parts to the system:

### Development side

The development machine parses and compiles the source code.

### Runtime side

The Arduino executes the generated NTOS bytecode.

This distinction is important.

**Acornima is part of the compiler frontend. It is not part of the Arduino runtime.**

---

# 🟨 Arduino JavaScript

One of the defining features of NTOS is its **Arduino JavaScript** programming model.

NTOS source code uses familiar JavaScript syntax:

```js
var playersCount = 4;

var balance = 2500;

if (balance > 2000) {
    print("Balance is high", GREEN);
}
```

But NTOS is **not JavaScript running on an Arduino**.

The compilation process is:

```text
                 main.ntos
                     │
                     ▼
              ┌────────────┐
              │  Acornima  │
              │ JS Parser  │
              └──────┬─────┘
                     │
                     ▼
              JavaScript AST
                     │
                     ▼
              ┌────────────┐
              │    NTOS    │
              │  Compiler  │
              └──────┬─────┘
                     │
                     ▼
              NTOS Bytecode
                     │
                     ▼
              ┌────────────┐
              │   NTOS VM  │
              └──────┬─────┘
                     │
                     ▼
                  Arduino
```

The Arduino never needs to understand JavaScript source code.

It only needs to understand NTOS bytecode.

This allows NTOS to use a mature JavaScript parser while keeping the compiler, bytecode, VM, memory model, runtime, and hardware interfaces completely under NTOS control.

---

# 🧩 The JavaScript Parser Under the Hood

NTOS does not use a hand-written parser for its source language.

The compiler uses **Acornima** to parse NTOS source code.

Acornima produces a JavaScript-compatible **Abstract Syntax Tree (AST)**.

The NTOS compiler then walks that AST and translates supported language constructs into NTOS instructions.

For example:

```js
if (playerBalance > 2000) {
    print("RICH", GREEN);
}
```

is not interpreted directly.

Conceptually:

```text
Source
  │
  ▼
Acornima
  │
  ▼
IfStatement
  │
  ├── BinaryExpression
  │      └── playerBalance > 2000
  │
  └── BlockStatement
         └── print(...)
  │
  ▼
NTOS AST Visitor
  │
  ▼
Jump / comparison / runtime-call instructions
```

This architecture makes the compiler easier to extend because language constructs can be implemented through AST visitors rather than requiring a new tokenizer and parser for every feature.

---

# ⚙️ Main Components

## NTOS Arduino

The embedded part of the project.

It contains:

* Virtual Machine
* Runtime functions
* Application loader
* SD-card storage
* Graphics system
* Input handling
* Application management
* Hardware integration

The Arduino is responsible for executing NTOS applications.

---

## NTOS Compiler

The compiler converts NTOS source code into custom bytecode.

It is written in C# and uses **Acornima** as its JavaScript parser frontend.

It handles:

* Variables
* Data types
* Expressions
* Arrays
* Functions
* Control flow
* Memory addresses
* Bytecode generation
* Runtime function calls
* Application compilation

The compiler operates on an AST rather than directly interpreting source text.

---

## NTOS Virtual Machine

The VM executes the generated bytecode on the Arduino.

Instead of compiling every application directly into Arduino firmware, NTOS executes a portable intermediate representation.

```text
NTOS Source
     │
     ▼
Acornima
     │
     ▼
JavaScript AST
     │
     ▼
NTOS Compiler
     │
     ▼
Bytecode
     │
     ▼
NTOS VM
     │
     ▼
Arduino
```

This separation is one of the core ideas behind NTOS.

---

# 💻 Arduino JavaScript Examples

NTOS source code is intentionally familiar to JavaScript developers while targeting the NTOS virtual machine.

## Variables

```js
var playersCount = 4;

var balance = 2500;

var playerName = "TIGRAN";

var enabled = true;

var key = 'A';
```

NTOS currently supports:

```text
int
float
bool
byte
string
```

Character literals are represented as bytes:

```js
var key = 'A';

if (key == 'A') {
    print("Pressed A", GREEN);
}
```

---

# ➕ Expressions

Arithmetic expressions are compiled into NTOS VM instructions.

```js
var a = 100;
var b = 50;

var total = a + b;
var difference = a - b;
var multiplied = total * 2;
var remainder = total % 3;
```

Compound assignments are also supported:

```js
balance += 500;
balance -= 100;
```

Comparisons can be used directly in control flow:

```js
if (balance >= 2000) {
    print("OK", GREEN);
}
```

---

# 📦 Arrays

Arrays can be declared directly from their values.

```js
var balances = [
    2500,
    2500,
    2500,
    2500
];

var names = [
    "P1",
    "P2",
    "P3",
    "P4"
];
```

The compiler automatically determines:

* Element type
* Number of elements
* Element size
* Required memory

Array indexing uses normal JavaScript syntax:

```js
var player = 2;

print(names[player], WHITE);
print(balances[player], GREEN);
```

Array elements can be modified:

```js
balances[2] = 3000;
names[2] = "TIGRAN";
```

Arrays are stored in the NTOS global memory space.

This means an array declared by one compiled bytecode unit can remain available to another compiled bytecode unit through its shared variable address.

---

# 🔁 Control Flow

NTOS supports normal JavaScript-style control flow.

## `if`

```js
if (balance > 2000) {
    print("RICH", GREEN);
}
```

## `while`

```js
var i = 0;

while (i < 10) {
    print(i, WHITE);

    i = i + 1;
}
```

## `do / while`

```js
var i = 0;

do {
    print(i, WHITE);

    i = i + 1;
}
while (i < 10);
```

## `for`

```js
for (var i = 0; i < 6; i = i + 1) {
    print(i, WHITE);
}
```

Nested loops are supported:

```js
for (var y = 0; y < 2; y = y + 1) {

    for (var x = 0; x < 3; x = x + 1) {

        if (x == y) {
            print("MATCH", GREEN);
        }
    }
}
```

The compiler converts these structures into NTOS jump instructions.

---

# 🔧 Functions and Subroutines

NTOS supports functions as bytecode subroutines.

```js
function drawPlayer() {
    print("PLAYER", WHITE);
    print(2500, GREEN);
}

drawPlayer();
```

Functions are compiled into NTOS bytecode.

The VM uses its subroutine and return-stack mechanism to execute them.

NTOS functions currently operate using the NTOS global memory model rather than JavaScript-style local function environments.

This keeps the runtime small and predictable for the target hardware.

---

# 🖥️ NTOS Runtime Functions

NTOS applications can call functions provided by the NTOS runtime.

For example:

```js
function draw() {

    dialog("MONETRIX BANK");

    cursor(20, 50, 2);

    print("Balance:", WHITE);

    print(2500, GREEN);
}
```

Runtime functions are compiled into NTOS VM function-call instructions.

The actual implementation runs inside the NTOS runtime on the Arduino.

This creates a clean separation:

```text
Arduino JavaScript
       │
       ▼
NTOS Compiler
       │
       ▼
NTOS VM Function Call
       │
       ▼
NTOS Runtime
       │
       ▼
Arduino Hardware
```

---

# 🎮 A More Complete Example

The following example combines variables, arrays, functions, loops, array indexing, conditions, runtime functions, and character input:

```js
var playersCount = 4;

var playerBalances = [
    2500,
    2500,
    2500,
    2500
];

var playerNames = [
    "P1",
    "P2",
    "P3",
    "P4"
];

function draw() {

    dialog("MONETRIX BANK");

    for (var i = 0; i < playersCount; i = i + 1) {

        cursor(20, 60 + i * 40, 2);

        print(playerNames[i], WHITE);

        print(" $", WHITE);

        print(playerBalances[i], GREEN);
    }
}

draw();

function loop() {

    var key = getKey();

    if (key == 'C') {

        playersCount = 2;

        draw();
    }

    delay(1);
}
```

This is still just source code.

The Arduino does not execute this JavaScript directly.

It becomes:

```text
main.ntos
    │
    ▼
Acornima
    │
    ▼
JavaScript AST
    │
    ▼
NTOS Compiler
    │
    ▼
NTOS Bytecode
    │
    ▼
SD Card
    │
    ▼
NTOS VM
    │
    ▼
Arduino TFT
```

---

# 🏦 Monetrix Example

One of the applications being developed with NTOS is **Monetrix Bank**.

A simplified version can look like:

```js
var playersCount = 4;

var playerBalance = [
    0,
    2500,
    2500,
    2500,
    2500,
    2500,
    2500
];

var playerNames = [
    "[BANK]",
    "P1",
    "P2",
    "P3",
    "P4",
    "P5",
    "P6"
];

function draw() {

    dialog("MONETRIX BANK");

    for (var y = 0; y <= 1; y = y + 1) {

        for (var x = 0; x <= 2; x = x + 1) {

            var i = x + y * 3 + 1;

            if (i <= playersCount) {

                cursor(
                    12 + x * 155,
                    60 + y * 80,
                    2
                );

                print(i, GRAY);
                print(":", GRAY);

                print(
                    playerNames[i],
                    WHITE
                );

                var color = YELLOW;

                if (playerBalance[i] < 0) {
                    color = RED;
                }

                if (playerBalance[i] > 200) {
                    color = GREEN;
                }

                cursor(
                    12 + x * 155,
                    85 + y * 80,
                    4
                );

                print("$", color);
                print(playerBalance[i], color);
            }
        }
    }
}

draw();

function loop() {

    var key = getKey();

    var selected = getNumericKey();

    if (key == 'C') {

        var pc = confirmNumber(
            "PLAYERS COUNT",
            "2 - 6",
            2,
            6
        );

        if (pc >= 0) {
            playersCount = pc;
        }

        draw();
    }

    if (key == 'A') {
        load("settings");
    }

    if (selected >= 0 &&
        selected <= playersCount) {

        load("target");
    }

    delay(1);
}
```

This type of application demonstrates the actual goal of NTOS:

**Write an application using a high-level language, compile it into bytecode, put it on an SD card, and let the Arduino execute it through the NTOS VM.**

---

# 💾 Applications on SD Card

Applications are designed to live independently from the NTOS firmware.

A typical application may look like:

```text
SD Card
│
├── Monetrix/
│   ├── main.ntx
│   ├── icon.nti
│   └── ...
│
├── Calculator/
│   ├── main.ntx
│   ├── icon.nti
│   └── ...
│
└── ...
```

The NTOS runtime can discover applications and load their bytecode from the SD card.

This allows the system to behave more like a traditional operating system rather than a collection of Arduino sketches.

---

# 🎨 NTOS Graphics

NTOS provides graphics functionality for applications running on the TFT display.

Applications can draw things such as:

* Pixels
* Lines
* Rectangles
* Text
* Images
* UI elements
* Graphs
* Custom interfaces

The graphics API is designed around the capabilities and limitations of the target hardware.

For example:

```js
cursor(20, 40, 2);

print("Hello NTOS", WHITE);
```

Or:

```js
cursor(20, 80, 4);

print("$2500", GREEN);
```

The application does not need to know how the TFT hardware is driven internally.

It calls the NTOS runtime.

---

# 🖼️ NTI Image Format

NTOS uses its own compact image format: **NTI**.

The format is intentionally simple:

```text
Byte 0      Width
Byte 1      Height
Byte 2+     RGB332 pixel data
```

Each pixel uses one byte:

```text
RRRGGGBB
```

This allows images to remain small and lets the Arduino stream image data directly from the SD card without requiring a large framebuffer.

For example:

```text
32 × 32 image

2 byte header
+
1024 bytes pixel data

=
1026 bytes
```

Small formats matter when the machine has very little RAM to work with.

---

# 🖥️ NTOS Dev

**NTOS Dev** is the development environment for NTOS.

It provides tools for:

* Writing NTOS applications
* Compiling source code
* Building applications
* Inspecting bytecode
* Managing application resources
* Creating NTI images
* Uploading applications
* Communicating with the Arduino
* Debugging and experimenting with NTOS

The goal is to make developing for NTOS feel less like repeatedly flashing Arduino firmware and more like developing applications for a small computer.

---

# 🖼️ NTOS Image Builder

The project includes a dedicated tool for converting images into NTI format.

```text
Input Image
     │
     ▼
┌───────────────┐
│ NTOS Image    │
│    Builder    │
├───────────────┤
│ Resize        │
│ Grayscale     │
│ Invert        │
│ Dithering     │
│ RGB332        │
└───────┬───────┘
        │
        ▼
     image.nti
```

The resulting image can be copied to the application's directory on the SD card.

---

# 🔌 Hardware

NTOS is designed around:

### Target Hardware

* **Arduino Mega**
* **Adafruit 3.5" TFT Shield**
* **SD card**
* TFT touchscreen/display hardware

The project is intentionally built around constrained hardware.

That limitation is part of the experiment.

Instead of hiding the complexity behind powerful hardware, NTOS has to deal with:

* Limited RAM
* Limited storage
* Slow SD access
* Serial communication
* Embedded graphics
* Small data types
* Processing limitations

Every byte has a job.

---

# 📦 File Types

| Extension | Purpose                |
| --------- | ---------------------- |
| `.ntos`   | NTOS source code       |
| `.ntx`    | Compiled NTOS bytecode |
| `.nti`    | NTOS image resource    |

Example:

```text
Monetrix/
├── main.ntx
├── icon.nti
└── ...
```

---

# 🔄 Build Pipeline

A typical NTOS application goes through this process:

```text
                 main.ntos
                     │
                     ▼
             ┌──────────────┐
             │   Acornima   │
             │ JavaScript   │
             │    Parser    │
             └──────┬───────┘
                    │
                    ▼
             JavaScript AST
                    │
                    ▼
             ┌──────────────┐
             │ NTOS Compiler│
             └──────┬───────┘
                    │
                    ▼
             NTOS Instructions
                    │
                    ▼
             ┌──────────────┐
             │   Bytecode   │
             │    Builder   │
             └──────┬───────┘
                    │
                    ▼
                 main.ntx
                    │
          ┌─────────┴─────────┐
          │                   │
          ▼                   ▼
       NTI files           Bytecode
          │                   │
          └─────────┬─────────┘
                    ▼
                 Build/
                    │
                    ▼
                 SD Card
                    │
                    ▼
                NTOS VM
                    │
                    ▼
               Application
```

The important part is that **Acornima is only used during compilation**.

Once the `.ntx` bytecode has been generated, the Arduino has no need for the JavaScript parser.

The runtime path is simply:

```text
NTOS Bytecode
      │
      ▼
   NTOS VM
      │
      ▼
   Arduino
```

---

# 🧪 Why Build This?

There are already many ways to run programs on microcontrollers.

NTOS exists primarily as a learning and experimentation project.

It provides an opportunity to explore the internals of:

* Compiler design
* Programming language design
* JavaScript parsing
* Abstract Syntax Trees
* Bytecode
* Virtual machines
* Runtime systems
* Memory management
* Embedded systems
* Graphics programming
* Storage systems
* Application loading
* Development environments

The project is essentially a long-running experiment in answering one question:

> **How much of a computer can you build yourself?**

---

# 🛠️ Project Structure

```text
ArduinoNTOS/
│
├── NTOSArduino/
│   ├── NTOS VM
│   ├── Runtime
│   ├── Storage
│   ├── Graphics
│   └── Hardware integration
│
├── NTOSCompiler/
│   ├── Compiler
│   └── Bytecode generation
│
├── NTOSDev/
│   ├── Development environment
│   ├── Emulator
│   └── Upload tools
│
└── NTOSImage/
    └── NTI image tools
```

---

# 📋 Current Features

### Compiler

* [x] Custom NTOS compiler
* [x] Acornima JavaScript parser frontend
* [x] JavaScript-compatible syntax
* [x] AST-based compilation
* [x] Custom bytecode generation
* [x] `var` declarations
* [x] `let` declarations
* [x] Global/shared variable memory
* [x] Integer values
* [x] Floating-point values
* [x] Boolean values
* [x] Byte values
* [x] String values
* [x] Character literals
* [x] Arrays
* [x] Array indexing
* [x] Array element assignment
* [x] Arithmetic expressions
* [x] Comparison operators
* [x] Logical operators
* [x] Unary expressions
* [x] Compound assignments
* [x] `if` / `else`
* [x] `while`
* [x] `do / while`
* [x] `for`
* [x] Functions
* [x] NTOS subroutines
* [x] Runtime function calls

### Runtime

* [x] Custom virtual machine
* [x] Bytecode execution
* [x] Runtime functions
* [x] Shared application memory
* [x] Application loading
* [x] SD-card application storage
* [x] Application management

### Graphics

* [x] TFT graphics
* [x] Text rendering
* [x] Images
* [x] NTI image format
* [x] NTI image builder
* [x] UI functionality

### Development

* [x] NTOS development environment
* [x] Bytecode inspection
* [x] Serial application upload
* [x] Arduino emulator/development tools

### In Progress

* [ ] More VM instructions
* [ ] Expanded application APIs
* [ ] More complete filesystem/application management
* [ ] Additional compiler features

---

# 🚧 Project Status

NTOS is an **experimental work in progress**.

The core compiler-to-VM pipeline is operational:

```text
NTOS Source
    ↓
Acornima
    ↓
JavaScript AST
    ↓
NTOS Compiler
    ↓
NTOS Bytecode
    ↓
NTOS VM
    ↓
Arduino
```

The compiler is now operating on a real parsed syntax tree rather than simply processing a custom collection of tokens.

This provides a much stronger foundation for adding language features.

The architecture continues to evolve as new features are added and new limitations are discovered.

That is intentional.

The project is being built from the bottom up, so sometimes adding one feature means discovering that another layer underneath it needs to evolve.

---

# 🎯 Long-Term Goal

The long-term goal is to create a small but complete environment where an Arduino can:

1. Boot NTOS
2. Read applications from an SD card
3. Display an application launcher
4. Load application bytecode
5. Execute applications through the VM
6. Access graphics and hardware through runtime APIs
7. Load application resources
8. Switch between applications

All running on an Arduino.

The development machine uses a real JavaScript parser.

The compiler converts the resulting AST into custom NTOS bytecode.

The Arduino executes that bytecode using the NTOS Virtual Machine.

There is **no JavaScript interpreter running on the Arduino**.

```text
             Arduino JavaScript
                     │
                     ▼
               ┌───────────┐
               │  Acornima  │
               │ JS Parser  │
               └─────┬─────┘
                     │
                     ▼
                JavaScript AST
                     │
                     ▼
               ┌───────────┐
               │    NTOS   │
               │  Compiler │
               └─────┬─────┘
                     │
                     ▼
               NTOS Bytecode
                     │
                     ▼
               ┌───────────┐
               │  NTOS VM  │
               └─────┬─────┘
                     │
                     ▼
                  Arduino
```

**Arduino JavaScript is the language frontend.**

**NTOS bytecode is the executable format.**

**The NTOS VM is the runtime.**

The parser may understand JavaScript.

The Arduino understands NTOS.

---

# 📜 License

Add your project license here.

For example:

```text
MIT License
```

Or replace this section with the license you choose.

---

# ⭐ Final Thought

NTOS started as an experiment to answer:

> **"Can I run an application from an SD card?"**

The rabbit hole went considerably deeper.

The answer became a compiler.

Then a parser.

Then a bytecode format.

Then a VM.

Then a runtime.

Then an application system.

Then a development environment.

And eventually:

**an operating system for an Arduino.**

```text
              JavaScript Syntax
                     │
                     ▼
                  Acornima
                     │
                     ▼
               JavaScript AST
                     │
                     ▼
                NTOS Compiler
                     │
                     ▼
                NTOS Bytecode
                     │
                     ▼
                  NTOS VM
                     │
                     ▼
                  Arduino
```

Welcome to **NTOS**. 🖥️⚙️

---

### NTOS Project

Built as an experiment in compilers, virtual machines, embedded systems, and the question:

> **How much of a computer can you build yourself?**
