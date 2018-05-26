CREATE TABLE SMSGateway(
	SMSGatewayID int AUTO_INCREMENT NOT NULL,
	SMSGatewayURL varchar(255) NOT NULL,
	SMSGatewayUserid varchar(255) NULL,
	SMSGatewayPwd varchar(50) NULL,
PRIMARY KEY 
(
	SMSGatewayID ASC
) 
);


CREATE TABLE smsenthistory(
	deviceid int NULL,
	sentdate date NULL,
	senttime varchar(15) NULL,
	statusid int NULL
); 

CREATE TABLE SLCFaultsValues(
	slcFaultValuesId int auto_increment,
	slcFaultId int NOT NULL,
	Value nvarchar(50) NOT NULL,
	Description nvarchar(50) NOT NULL,
 CONSTRAINT PK_SLCFaultsValues PRIMARY KEY 
(
	slcFaultValuesId ASC
) 
);

CREATE TABLE SLCFaults(
	SLCFaultid int auto_increment,
	SLCFaultName nvarchar(50) NOT NULL,
	SLCFaultDescription nvarchar(50) NOT NULL,
	SLCFaultSeverity nvarchar(50) NOT NULL,
	Other int NOT NULL,
	gf int NOT NULL,
	Phase_1 int NOT NULL,
	Phase_3 int NOT NULL,
 CONSTRAINT PK_SlcfaultDetails PRIMARY KEY 
(
	SLCFaultid ASC
) 
);

ALTER TABLE smsauthority ADD smsGatewayID int null;






