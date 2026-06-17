Add-Type -AssemblyName PresentationCore
Add-Type -AssemblyName WindowsBase

$pathData = "M6,2H18A2,2 0 0,1 20,4V20A2,2 0 0,1 18,22H6A2,2 0 0,1 4,20V4A2,2 0 0,1 6,2M12.75,13.5C15.5,13.5 16.24,11.47 16.43,10.4C17.34,10.11 18,9.26 18,8.25C18,7 17,6 15.75,6C14.5,6 13.5,7 13.5,8.25C13.5,9.19 14.07,10 14.89,10.33C14.67,11 14,12 12,12C10.62,12 9.66,12.35 9,12.84V8.87C9.87,8.56 10.5,7.73 10.5,6.75C10.5,5.5 9.5,4.5 8.25,4.5C7,4.5 6,5.5 6,6.75C6,7.73 6.63,8.56 7.5,8.87V15.13C6.63,15.44 6,16.27 6,17.25C6,18.5 7,19.5 8.25,19.5C9.5,19.5 10.5,18.5 10.5,17.25C10.5,16.32 9.94,15.5 9.13,15.18C9.41,14.5 10.23,13.5 12.75,13.5M8.25,16.5A0.75,0.75 0 0,1 9,17.25A0.75,0.75 0 0,1 8.25,18A0.75,0.75 0 0,1 7.5,17.25A0.75,0.75 0 0,1 8.25,16.5M8.25,6A0.75,0.75 0 0,1 9,6.75A0.75,0.75 0 0,1 8.25,7.5A0.75,0.75 0 0,1 7.5,6.75A0.75,0.75 0 0,1 8.25,6M15.75,7.5A0.75,0.75 0 0,1 16.5,8.25A0.75,0.75 0 0,1 15.75,9A0.75,0.75 0 0,1 15,8.25A0.75,0.75 0 0,1 15.75,7.5Z"

$geometry = [System.Windows.Media.PathGeometry]::Parse($pathData)

# Yellow matching the app's folder icon colour
$brush = New-Object System.Windows.Media.SolidColorBrush(
    [System.Windows.Media.Color]::FromRgb(0xFF, 0xB9, 0x00))

function Render-Png([int]$size) {
    $scale  = $size / 24.0
    $xform  = New-Object System.Windows.Media.ScaleTransform($scale, $scale)

    $dv = New-Object System.Windows.Media.DrawingVisual
    $dc = $dv.RenderOpen()
    $dc.PushTransform($xform)
    $dc.DrawGeometry($brush, $null, $geometry)
    $dc.Pop()
    $dc.Close()

    $rtb = New-Object System.Windows.Media.Imaging.RenderTargetBitmap(
        $size, $size, 96, 96,
        [System.Windows.Media.PixelFormats]::Pbgra32)
    $rtb.Render($dv)

    $enc = New-Object System.Windows.Media.Imaging.PngBitmapEncoder
    $enc.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($rtb))
    $ms = New-Object System.IO.MemoryStream
    $enc.Save($ms)
    return $ms.ToArray()
}

$sizes   = @(16, 24, 32, 48, 64, 128, 256)
$pngList = $sizes | ForEach-Object { , (Render-Png $_) }

# Pack into ICO (ICONDIR + ICONDIRENTRY[] + PNG blobs)
$ico    = New-Object System.IO.MemoryStream
$count  = $sizes.Count
$offset = 6 + $count * 16

# ICONDIR header
$ico.WriteByte(0); $ico.WriteByte(0)   # reserved
$ico.WriteByte(1); $ico.WriteByte(0)   # type = 1 (icon)
$ico.WriteByte([byte]$count); $ico.WriteByte(0)

# ICONDIRENTRY for each size
for ($i = 0; $i -lt $count; $i++) {
    $w    = if ($sizes[$i] -ge 256) { 0 } else { $sizes[$i] }
    $len  = $pngList[$i].Length

    $ico.WriteByte([byte]$w)   # width  (0 = 256)
    $ico.WriteByte([byte]$w)   # height (0 = 256)
    $ico.WriteByte(0)           # colour count
    $ico.WriteByte(0)           # reserved
    $ico.WriteByte(1); $ico.WriteByte(0)   # planes
    $ico.WriteByte(32); $ico.WriteByte(0)  # bits per pixel

    foreach ($shift in 0,8,16,24) { $ico.WriteByte([byte](($len    -shr $shift) -band 0xFF)) }
    foreach ($shift in 0,8,16,24) { $ico.WriteByte([byte](($offset -shr $shift) -band 0xFF)) }

    $offset += $len
}

# PNG blobs
foreach ($png in $pngList) { $ico.Write($png, 0, $png.Length) }

$outPath = "D:\BradWilson\projects\GitRepoScanner\GitRepoScanner\app.ico"
[System.IO.File]::WriteAllBytes($outPath, $ico.ToArray())
Write-Host "Written: $outPath ($([Math]::Round($ico.Length/1KB, 1)) KB)"
