# SQLServerUtils
Browse and Search SQL Server Ojbects quickly on a Wndows PC using your browser localhost:8084

Project is forked from https://github.com/usharik/MsSqlDependencyBrowser. Original project was written in Angular js and I replaced it with Vue.js and also replaced bunch of C# logic to fit my needs.


The project has two seperate parts!
- C# console application used as a simple webapp served at **localhost:8084**
- Vue js web application used to develop index.html and javascript components.
- A powershell script (in Utils folder) is used to copy vue compiled js,css and html into c# console app


Example Screen Print
![alt text](https://github.com/ali4728/SQLServerUtils/blob/master/Utils/img/Navigate_Stored_Procs.PNG)


## Setup
Download binaries from **~\bin\debug** folder
Update path in **SQLServerUtils.exe.config** file for below items 
  ```
  <add key="ServerListFilePath" value="C:\temp\serverlist.txt" />
  <add key="DiffPath" value="C:\temp\diff" />
  ```
    
 **serverlist.txt file** contains list of sql server machine names. One server name in each line.
 
 # DIFF
 Diff functionality is useful if you would like to compare stored proc (or other objects) in **test** to stored proc in **prod** environments quickly.
 
 Update SQLServerUtils/diff/diff.bat if you would like to use WinMerge for **Diff** functionality.
 
 
    
