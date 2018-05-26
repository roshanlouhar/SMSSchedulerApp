<?php

/* This php file contains the methods which access the ESMS web services
*Author: Akalanka De Silva
*
*
*=================sample code for sending SMS===========================================
*
*$session=createSession('','username','password','');
*sendMessages($session,'alias','message text',array('71xxxxxxx','71xxxxxxx'));
*closeSession($session);
*
*=======================================================================================
*
*
*==============sample code for retrieving SMS===========================================
*
*$session=createSession('','username','password','');
*getMessagesFromShortCode($session,"shortcode");
*getMessagesFromLongNumber($session,"longnum");
*closeSession($session);
*
*=======================================================================================
**/

//====================================ESMS WEB SERVCIES START	================================



//create soap client
function getClient(){

	ini_set("soap.wsdl_cache_enabled", "0");
	$client = new SoapClient("http://smeapps.mobitel.lk:8585/EnterpriseSMS/EnterpriseSMSWS.wsdl");
	return $client;

}

//serviceTest
function serviceTest($id,$username,$password,$customer){

	$client=getClient();

	$user = new stdClass();
	$user->id = '';
	$user->username = $username;
	$user->password = $password;
	$user->customer = '';

	$serviceTest=new stdClass();
	$serviceTest->arg0=$user;

	return $client->serviceTest($serviceTest);

}

//create session
function createSession($id,$username,$password,$customer){

	$client=getClient();

	$user=new stdClass();
	$user->id=$id;
	$user->username=$username;
	$user->password=$password;
	$user->customer=$customer;

	$createSession=new stdClass();
	$createSession->arg0=$user;

	$createSessionResponse=new stdClass();
	$createSessionResponse= $client->createSession($createSession);

	return $createSessionResponse->return;

}

//check if session is valid
function isSession($session){

	$client=getClient();

	$isSession= new stdClass();
	$isSession->arg0=$session;

	$isSessionResponse=new stdClass();
	$isSessionResponse= $client->isSession($isSession);

	return $isSessionResponse->return;
}

//send SMS to recipients
function sendMessages($session,$alias,$message,$recipients){

	$client=getClient();

	$aliasObj=new stdClass();
	$aliasObj->alias=$alias;
	$aliasObj->customer="";
	$aliasObj->id="";


	$smsMessage= new stdClass();
	$smsMessage->message=$message;
	$smsMessage->messageId="";
	$smsMessage->recipients=$recipients;
	$smsMessage->retries="";
	$smsMessage->sender=$aliasObj;
	$smsMessage->sequenceNum="";
	$smsMessage->status="";
	$smsMessage->time="";
	$smsMessage->type="";
	$smsMessage->user="";

	$sendMessages=new stdClass();
	$sendMessages->arg0=$session;
	$sendMessages->arg1=$smsMessage;

	$sendMessagesResponse=new stdClass();
	$sendMessagesResponse=$client->sendMessages($sendMessages);

	return $sendMessagesResponse->return;
}

//renew session 
function renewSession($session){

	$client=getClient();

	$renewSession=new stdClass();
	$renewSession->arg0=$session;

	$renewSessionResponse=new stdClass();
	$renewSessionResponse=$client->renewSession($renewSession);

	return $renewSessionResponse->return;

}


//close session
function closeSession($session){

	$client=getClient();

	$closeSession=new stdClass();
	$closeSession->arg0=$session;

	$client->closeSession($closeSession);

}

//retrieve messages from shortcode
function getMessagesFromShortCode($session,$shortCode){

$client=getClient();

	$shortCodeObj=new stdClass();
	$shortCodeObj->shortcode=$shortCode;

	$getMessagesFromShortCode=new stdClass();
	$getMessagesFromShortCode->arg0=$session;
	$getMessagesFromShortCode->arg1=$shortCodeObj;

	$getMessagesFromShortcodeResponse=new stdClass();
	$getMessagesFromShortcodeResponse->return="";
	$getMessagesFromShortcodeResponse=$client->getMessagesFromShortcode($getMessagesFromShortCode);
	
	if(property_exists($getMessagesFromShortcodeResponse,'return'))
	return $getMessagesFromShortcodeResponse->return;
	
	else return NULL;

}

//retrieve delivery report
function getDeliveryReports($session,$alias){

$client=getClient();

	$aliasObj=new stdClass();
	$aliasObj->alias=$alias;

	$getDeliveryReports=new stdClass();
	$getDeliveryReports->arg0=$session;
	$getDeliveryReports->arg1=$aliasObj;

	$getDeliveryReportsResponse=new stdClass();
	$getDeliveryReportsResponse->return="";
	$getDeliveryReportsResponse=$client->getDeliveryReports($getDeliveryReports);
	
	if(property_exists($getDeliveryReportsResponse,'return'))
	return $getDeliveryReportsResponse->return;
	
	else return NULL;

}

//retrieve messages from longnumber
function getMessagesFromLongNumber($session,$longNumber){

	$client=getClient();

	$longNumberObj=new stdClass();
	$longNumberObj->longNumber=$longNumber;

	$getMessagesFromLongNUmber=new stdClass();
	$getMessagesFromLongNUmber->arg0=$session;
	$getMessagesFromLongNUmber->arg1=$longNumberObj;

	$getMessagesFromLongNUmberResponse=new stdClass();
	$getmessagesFromLongNUmberResponse->return="";
	$getMessagesFromLongNUmberResponse=$client->getMessagesFromLongNUmber($getMessagesFromLongNUmber);
	
	return $getMessagesFromLongNUmberResponse->return;
}




//==================================ESMS WEB SERVICE END=============================================================



?>

