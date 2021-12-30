Add-Type -Path ..\LehmanLaidun.FileSystem\bin\Debug\netstandard2.0\LehmanLaidun.FileSystem.dll
# $x = New-Object TheProject.Controller
$x = [LehmanLaidun.FileSystem.LogicFactory]::CreateForPath($null, 'a', $null)
# $x.Scan('asd')
$x