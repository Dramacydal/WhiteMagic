WhiteMagic
=========

API to work with Win32 process memory and hardware breakpoints

Targets .NET 10. Uses Iced (https://github.com/icedland/iced) to assemble
remote-call shellcode.


Capabilities:
* Read/write process memory 
* Suspend/resume process thread(s)
* Call remote process functions or assembled code (using remote thread injection)
* Attach hardware breakpoints to remote processes
* Search for data patterns in remote process' memory
* Hooks mouse and keyboard events
* ...
