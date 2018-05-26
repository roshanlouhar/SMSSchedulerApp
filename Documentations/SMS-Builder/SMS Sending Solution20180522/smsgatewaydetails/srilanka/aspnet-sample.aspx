<%@ Page Language="VB" %> 
<%@ Import Namespace="System.Net" %> 
<%@ Import Namespace="System.IO" %> 

<SCRIPT Language="VB" Option="Explicit" runat="server"> 

Function SendSMS(strRecip as String, strMsgText as String) As String 

Dim objURI As URI = New URI("http://www.quicksms.in/sendsms.asp?user=username&password=password&sender=Quicksms.in&" _ 
& "PhoneNumber=" & strRecip _ 
& "&Text=" & HttpUtility.URLEncode(strMsgText)) 
Dim objWebRequest As WebRequest = WebRequest.Create(objURI) 
Dim objWebResponse As WebResponse = objWebRequest.GetResponse() 
Dim objStream As Stream = objWebResponse.GetResponseStream() 
Dim objStreamReader As StreamReader = New StreamReader(objStream) 
Dim strHTML As String = objStreamReader.ReadToEnd 

SendSMS = strHTML 

End Function 

</SCRIPT> 

<%= SendSMS( "919422251514", "Test Message") %>