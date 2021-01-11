# SQLServerUtils
Browse and Search SQL Server Ojbects quickly on a Wndows PC using your browser. It is a fork from [MsSqlDependencyBrowser](https://github.com/usharik/MsSqlDependencyBrowser).

# Functionality
- Connect to SQL Server machines and databases quickly by selecting from drop down lists
- Realtime filter of objects (tables, stored proc, view etc.) with a few key strokes
- Single click preview of object text (sp,view etc.)
- Search stored procedure code in multiple servers and multiple databases at once/parallel (use threading)
- Search column names, table names and other object types.
- Quickly Diff between prod and test object (sp, view etc) using WinMerge in the backend


# Implementation Details
Project is forked from https://github.com/usharik/MsSqlDependencyBrowser. Original project was written in Angular js and I replaced it with Vue.js and also replaced bunch of C# logic to fit my needs.


The project has two seperate parts!
- C# console application used as a simple webapp server, served at **localhost:8084**
- Vue js web application used to develop javascript, css and html components.
- A powershell script (in Utils folder) is used to copy vue compiled js, css and html into c# console app

# Setup
- Download binaries from **~\bin\debug** folder
- Copy **~\SQLServerUtils\Web** and **~\SQLServerUtils\Diff** folders and their contents into bin folder
- Update path in **SQLServerUtils.exe.config** file for below items
- Double click to run **~\bin\Debug\SQLServerUtils.exe** program

  ```
  <add key="ServerListFilePath" value="C:\temp\serverlist.txt" />
  <add key="DiffPath" value="C:\temp\diff" />
  ```
    
 **serverlist.txt** file contains list of sql server machine names. One server name on each line.
 
 
 # Diff
 Diff functionality is useful if you would like to compare stored proc (or other objects) in **test** to stored proc in **prod** environments quickly.
 
 Update **~\SQLServerUtils\diff\diff.bat** if you would like to use WinMerge for **Diff** functionality.
 
 
 
# Screen Prints
![MainPage](https://github.com/ali4728/SQLServerUtils/blob/master/Utils/img/Navigate_Stored_Procs.PNG)

![Diff](https://github.com/ali4728/SQLServerUtils/blob/master/Utils/img/Diff.PNG)

![Search](https://github.com/ali4728/SQLServerUtils/blob/master/Utils/img/Search.PNG)

