Do not download old DLL packs from random repositories.

Run build.ps1 with your CURRENT SCP:SL Dedicated Server directory. The script copies:
- Assembly-CSharp.dll
- LabApi.dll / LabAPI.dll
- UnityEngine.CoreModule.dll

from SCPSL_Data\Managed into this directory before building.

This project intentionally does NOT require:
- MEC.dll
- Assembly-CSharp-firstpass.dll
- EXILED
- NwPluginAPI
