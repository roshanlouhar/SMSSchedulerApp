<?php

// Call this function when ever you wish to Send SMS

$res=sendsms ("919422251514","Hello this is a test SMS message");
//Check Result of Sending SMS

if ($res==1)
	echo "SMS 1 was successful";
else
	echo "sms  1 was not successful";

function sendsms($mobile,$text)
	{
	$host = "www.quicksms.in";
	$request = ""; 					//initialize the request variable
	$param["user"] = "username"; 		//	this is the username of your SMS account
	$param["password"] = "password"; 	//this is the password of our SMS account
	$param["text"] = substr($text,0,159); 	//this is the message that we want to send
	$param["PhoneNumber"] = $mobile; 		//these are the recipients of the message
	$tmp=substr($mobile,0,4);
	
	if ($tmp=="9198" || $tmp=="9194" || $tmp=="9199" || $tmp=="9192" || $tmp=="9197")
		$sender="Quicksms.in";			// SenderID for GSM and TATA
	else
		$sender="919860609000";		// SenderID for CDMA (Reliance0
	$param["sender"] = $sender;
	foreach($param as $key=>$val) 	// Traverse through each member of the param array
		{ 
	  	$request.= $key."=".urlencode($val); //we have to urlencode the values
	  	$request.= "&"; //append the ampersand (&) sign after each paramter/value pair
		}
	$request = substr($request, 0, strlen($request)-1); //remove the final ampersand sign from the request
	$script = "/sendsms.asp";
	$request_length = strlen($request);
	$method = "POST"; 
	if ($method == "GET") 
		{
 		$script .= "?$request";
		}
	//Now comes the header which we are going to post. 
	$header = "$method $script HTTP/1.1\r\n";
	$header .= "Host: $host\r\n";
	$header .= "Content-Type: application/x-www-form-urlencoded\r\n";
	$header .= "Content-Length: $request_length\r\n";
	$header .= "Connection: close\r\n\r\n";
	$header .= "$request\r\n";

	//Now we open up the connection
	$socket = @fsockopen($host, 80, $errno, $errstr); 
	if ($socket) //if its open, then...
		{ 
	  	fputs($socket, $header); // send the details over
	  	while(!feof($socket))
	  		{
			$output=$output.fgets($socket); //get the results 
	  		}
	  	fclose($socket); 
		} 

	$pos=strpos($output,"Submitted");
	if ($pos===false)
		return(0);  // SMS Not Successful
	else
		return(1); // SMS Was successful
	}
?>
