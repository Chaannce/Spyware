<?php
$ip = $_SERVER['REMOTE_ADDR'];
$ipInfo = json_decode(file_get_contents('http://ip-api.com/json/'.$ip));
echo "IP: ".$ipInfo->query;
echo "<br>";
echo "Country Code: ".$ipInfo->countryCode;
echo "<br>";
echo "Region: ".$ipInfo->region;
echo "<br>";
echo "City: ".$ipInfo->city;
echo "<br>";
echo "Zip: ".$ipInfo->zip;
echo "<br>";
echo "Lon, Lat: (".$ipInfo->lon.", ".$ipInfo->lat.")";
?>
