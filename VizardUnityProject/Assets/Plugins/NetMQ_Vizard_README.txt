Notes on the NetMQ upgrade of 3/23/23. I did my best to pick Net45 dll, but sometimes I had to pick the closest thing available. I had to add a lot of new dll to get Vizard to compile with the upgraded NetMQ. All of these dll archives were obtained from nuGET.

NetMQ. (netmq.4.0.1.11 archive) chose the dll targeting Net45
https://www.nuget.org/packages/NetMQ/

AsynchIO (asyncio.0.1.69 archive) chose the dll targeting Net40
https://www.nuget.org/packages/NetMQ/

NaCl  (nacl.net.0.1.13 archive) chose the dll targeting Net45 *
https://www.nuget.org/packages/NaCl.Net/

System.Buffers. (system.buffers.4.5.1) chose dll targeting net461 *
https://www.nuget.org/packages/System.Buffers/

System.Memory (system.memory.4.5.5) chose dll targeting net461 *
https://www.nuget.org/packages/System.Memory

System.Runtime.CompilerServices.Unsafe (system.runtime.compilerservices.unsafe.6.0.0) chose dll targeting net461 *
https://www.nuget.org/packages/System.Runtime.CompilerServices.Unsafe/

System.Threading.Tasks.Extensions (system.threading.tasks.extensions.4.5.4) chose dll targeting net461 *
https://www.nuget.org/packages/System.Threading.Tasks.Extensions/


* new addition to Vizard plugins folder
