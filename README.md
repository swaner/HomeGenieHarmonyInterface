# MIG-Harmony interface
HomeGenie / MIG interface driver for Harmony. Tested on HarmonyHub on Raspberry Pi 2 and Windows 10.

##Installation
Install in HomeGenie, use the package "mig_interface_harmony_zip"
Stop HomeGenie, open systemconfig.xml and set options at the bottom of the page:
```xml
<Interface Domain="HomeAutomation.Harmony" IsEnabled="true" AssemblyName="Harmony.dll">
    <Options>
      <Option Name="Username" Value="email" />
      <Option Name="Password" Value="pass" />
      <Option Name="IPAddress" Value="10.0.0.6" />
    </Options>
</Interface>
```
Start HomeGenie
Goto configura modules and add the wanted activities


##Features
Currently the interface supports the following:
* Reading all harmony activities and adding them to HomeGenie
* Starting and stopping activities
