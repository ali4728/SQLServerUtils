

$source = "F:\Shared\Ali\dev\js\vue_apps\vue_20201223\sqlpal\dist"
$destination = "C:\Users\ali4728\Documents\Visual Studio 2017\Projects\SQLServerUtils\SQLServerUtils\Web"



$cssOld = '(href=\/css\/app\.[A-Za-z0-9]+)\.css'   #'href=/css/app.489daa2e.css'
$cssNew = 'href=/main.css'

$cssOldVendor = '(href=\/css\/chunk-vendors\.[A-Za-z0-9]+)\.css'   #'href=/css/app.489daa2e.css'
$cssNewVendor = 'href=/vendor.css'

$jsOneOld = '(href=\/js\/app\.[A-Za-z0-9]+)\.js' #'href=/js/app.5faf4ecd.js'
$jsOneNew = 'href=/app.js'

$jsTwoOld = '(href=\/js\/chunk-vendors\.[A-Za-z0-9]+)\.js'  #'href=/js/chunk-vendors.6d69e8c9.js'
$jsTwoNew = 'href=/vendor.js'



$jsBottomOneOld = '(src=\/js\/chunk-vendors\.[A-Za-z0-9]+)\.js'   #'src=/js/chunk-vendors.6d69e8c9.js'
$jsBottomOneNew = 'src=/vendor.js'

$jsBottomTwoOld = '(src=\/js\/app\.[A-Za-z0-9]+)\.js' #'src=/js/app.5faf4ecd.js'
$jsBottomTwoNew = 'src=/app.js'



#href=/css/app.489daa2e.css   href=/main.css
#href=/js/app.5faf4ecd.js     href=/app.js rel=preload

#link href=/js/chunk-vendors.6d69e8c9.js                 link href=/vendor.js
#<script src=/js/chunk-vendors.6d69e8c9.js></script>     <script src=/vendor.js></script>
#<script src=/js/app.5faf4ecd.js></script>               <script src=/app.js></script>






function FindReplace($old, $new, $fpath)
{
	(Get-Content $fpath) |
		Foreach-Object { $_ -replace $old, $new } |
		Set-Content $fpath
	
		$comment = "old: {0} New: {1} File: {2}" -f $old, $new, $fpath		
		#Write-Output $comment 
}






Write-Output $fsource
Write-Output $fdest


$fsource = "{0}\{1}" -f $source, "index.html"
$fdest = "{0}\{1}" -f $destination, "index.html"


Copy-Item $fsource  -Destination $destination #copy index




#copy css
$fsourceCSS = "{0}\{1}" -f $source, "css"

 foreach ($item in Get-ChildItem $fsourceCSS  ) #-Filter "*.map"
 {
    $fnameNoPath = [System.IO.Path]::GetFileName($item.FullName)	
    
    if($fnameNoPath -Match '(app\.[A-Za-z0-9]+)\.css$') 
    {
        $destFile = "{0}\{1}" -f $destination, "main.css"
        Copy-Item $item.FullName  -Destination $destFile #copy index
        Write-Output "$fnameNoPath copyed to $destFile  "    
    }

    if($fnameNoPath -Match '(chunk-vendors\.[A-Za-z0-9]+)\.css$')  
    {
        $destFile = "{0}\{1}" -f $destination, "vendor.css"
        Copy-Item $item.FullName  -Destination $destFile #copy index
        Write-Output "$fnameNoPath copyed to $destFile  "    
    }
 }




FindReplace $cssOld $cssNew $fdest
FindReplace $cssOldVendor $cssNewVendor $fdest
FindReplace $jsOneOld $jsOneNew $fdest
FindReplace $jsTwoOld $jsTwoNew $fdest

FindReplace $jsBottomTwoOld $jsBottomOneNew $fdest
FindReplace $jsBottomOneOld $jsBottomTwoNew $fdest


 $jsFolder = "{0}\{1}" -f $source, "js"

 foreach ($item in Get-ChildItem $jsFolder  ) #-Filter "*.map"
 {
    $fnameNoPath = [System.IO.Path]::GetFileName($item.FullName)	
    #$fnwop = [System.IO.Path]::GetFileName($item.FullName)
    
    
    if($fnameNoPath -Match '(app\.[A-Za-z0-9]+)\.js$') 
    {
        #app js
        

        $destFile = "{0}\{1}" -f $destination, "app.js"
        Copy-Item $item.FullName  -Destination $destFile #copy index
        Write-Output "$fnameNoPath copyed to $destFile  "



        FindReplace '(app\.[A-Za-z0-9]+)\.js\.map$' 'app.map' $destFile

    }

    if($fnameNoPath -Match '(app\.[A-Za-z0-9]+)\.js\.map$') 
    {
        #app map
        
        $destFile = "{0}\{1}" -f $destination, "app.map"
        Copy-Item $item.FullName  -Destination $destFile #copy index
        Write-Output "$fnameNoPath copyed to $destFile  "
        
    }

    
    if($fnameNoPath -Match '(chunk-vendors\.[A-Za-z0-9]+)\.js$') 
    {
        #vendor js
        $destFile = "{0}\{1}" -f $destination, "vendor.js"
        Copy-Item $item.FullName  -Destination $destFile #copy index
        Write-Output "$fnameNoPath copyed to $destFile  "


        FindReplace '(chunk-vendors\.[A-Za-z0-9]+)\.js\.map$' 'vendor.map' $destFile
        
    }

   if($fnameNoPath -Match '(chunk-vendors\.[A-Za-z0-9]+)\.js\.map$') 
   {
        $destFile = "{0}\{1}" -f $destination, "vendor.map"
        Copy-Item $item.FullName  -Destination $destFile #copy index
        Write-Output "$fnameNoPath copyed to $destFile  "
       
   }
    

 }