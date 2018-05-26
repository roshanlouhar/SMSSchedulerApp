Attribute VB_Name = "General"
Option Explicit

Public Const KEYEVENTF_KEYUP = &H2
Public Const IF_FROM_CACHE = &H1000000
Public Const IF_MAKE_PERSISTENT = &H2000000
Public Const IF_NO_CACHE_WRITE = &H4000000

Public Declare Function WritePrivateProfileString Lib "kernel32" Alias "WritePrivateProfileStringA" (ByVal lpApplicationName As String, ByVal lpKeyName As Any, ByVal lpString As Any, ByVal lpFileName As String) As Long
Public Declare Function GetPrivateProfileString Lib "kernel32" Alias "GetPrivateProfileStringA" (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpDefault As String, ByVal lpReturnedString As String, ByVal nSize As Long, ByVal lpFileName As String) As Long
Public Declare Function InternetOpen Lib "wininet.dll" Alias "InternetOpenA" (ByVal sAgent As String, ByVal lAccessType As Long, ByVal sProxyName As String, ByVal sProxyBypass As String, ByVal lFlags As Long) As Long
Public Declare Function InternetOpenUrl Lib "wininet.dll" Alias "InternetOpenUrlA" (ByVal hInternetSession As Long, ByVal sURL As String, ByVal sHeaders As String, ByVal lHeadersLength As Long, ByVal lFlags As Long, ByVal lContext As Long) As Long
Public Declare Function InternetReadFile Lib "wininet.dll" (ByVal hFile As Long, ByVal sBuffer As String, ByVal lNumBytesToRead As Long, lNumberOfBytesRead As Long) As Integer
Public Declare Function InternetCloseHandle Lib "wininet.dll" (ByVal hInet As Long) As Integer

Public Property Get ReadINI(StrSection As String, StrKey As String, ValDef As String) As String
   Dim strbuffer As String
   Let strbuffer$ = String$(750, Chr$(0&))
   Let ReadINI$ = Left$(strbuffer$, GetPrivateProfileString(StrSection$, ByVal LCase$(StrKey$), "", strbuffer, Len(strbuffer), App.Path & "\sms.ini"))
   If ReadINI = "" Then
        WriteINI StrSection, StrKey, ValDef
        ReadINI = ValDef
    End If
End Property

Public Sub WriteINI(StrSection As String, StrKey As String, strkeyvalue As String)
    Call WritePrivateProfileString(StrSection$, UCase$(StrKey$), strkeyvalue$, App.Path & "\sms.ini")
End Sub



Public Function GetUrlSource(sURL As String) As String
    Const BUFFER_LEN = 256
    Dim sBuffer As String * BUFFER_LEN, iResult As Integer, sData As String
    Dim hInternet As Long, hSession As Long, lReturn As Long

    DoEvents
    hSession = InternetOpen("vb wininet", 1, vbNullString, vbNullString, 0)
    If hSession Then hInternet = InternetOpenUrl(hSession, sURL, vbNullString, 0, IF_NO_CACHE_WRITE, 0)
    If hInternet Then
        DoEvents
        iResult = InternetReadFile(hInternet, sBuffer, BUFFER_LEN, lReturn)
        sData = sBuffer
        Do While lReturn <> 0
            DoEvents
            iResult = InternetReadFile(hInternet, sBuffer, BUFFER_LEN, lReturn)
            sData = sData + Mid(sBuffer, 1, lReturn)
            DoEvents
        Loop
    End If
   

    iResult = InternetCloseHandle(hInternet)

    GetUrlSource = sData
End Function


Function URLEncode(Data)
  Dim i, c, Out
  
  For i = 1 To Len(Data)
    c = Asc(Mid(Data, i, 1))
    If c = 32 Then
      Out = Out + "+"
    ElseIf c < 48 Then
      Out = Out + "%" + Hex(c)
    Else
      Out = Out + Mid(Data, i, 1)
    End If
    DoEvents
  Next
  URLEncode = Out
End Function


