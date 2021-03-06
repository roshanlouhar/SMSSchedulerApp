CREATE TABLE cp(
	_cpid_ int NOT NULL,
	_deviceid_ int NOT NULL,
	_name_ nvarchar(50) NOT NULL,
	_address_ nvarchar(50) NOT NULL,
	_mobile_ nvarchar(50) NOT NULL,
 CONSTRAINT PK_cp PRIMARY KEY 
(
	_cpid_ ASC
) 
);

CREATE TABLE slc_devices(
	_deviceid_ int NOT NULL,
	_dt_ nvarchar(50) NOT NULL,
	_name_ nvarchar(50) NOT NULL,
	_switchno_ int NOT NULL,
	_Location_ nvarchar(50) NOT NULL,
	_area_ nvarchar(50) NOT NULL,
	_rating_ Double NOT NULL,
	_totalload_ Double NOT NULL,
	_totalload_b_ Double NOT NULL,
	_totalload_y_ Double NOT NULL,
	_numberofpoles_ Double NOT NULL,
	_numberofpoles_b_ Double NOT NULL,
	_numberofpoles_y_ Double NOT NULL,
	_referencewattage_b_ Double NOT NULL,
	_referencewattage_y_ Double NOT NULL,
	_referencewattage_ Double NOT NULL,
	_typeofload_ nvarchar(50) NOT NULL,
	_latitude_ Double NOT NULL,
	_longitude_ Double NOT NULL,
	_mobile_ Double NOT NULL,
	_city_ int NOT NULL,
	_userid_ int NOT NULL,
	_phase_ int NOT NULL,
	_wardno_ Double NOT NULL,
	_zone_ nvarchar(50) NOT NULL,
	_commandmode_ int NOT NULL,
	_updated_ int NOT NULL,
	_cpname_ nvarchar(50) NOT NULL,
	_cpmobileno_ nvarchar(50) NOT NULL
);

CREATE TABLE slcevents(
	_id_ nvarchar(50) NOT NULL,
	_slcid_ nvarchar(50) NOT NULL,
	_deviceid_ nvarchar(50) NOT NULL,
	_dt_ nvarchar(50) NOT NULL,
	_aux1_ nvarchar(50) NOT NULL,
	_aux2_ nvarchar(50) NOT NULL,
	_aux3_ nvarchar(50) NOT NULL,
	_aux4_ nvarchar(50) NOT NULL,
	_aux5_ nvarchar(50) NOT NULL,
	_aux6_ nvarchar(50) NOT NULL,
	_aux7_ nvarchar(50) NOT NULL,
	_aux8_ nvarchar(50) NOT NULL,
	_r_failure_ nvarchar(50) NOT NULL,
	_y_failure_ nvarchar(50) NOT NULL,
	_b_failure_ nvarchar(50) NOT NULL,
	_r_ nvarchar(50) NOT NULL,
	_y_ nvarchar(50) NOT NULL,
	_b_ nvarchar(50) NOT NULL,
	_fault_ov_ nvarchar(50) NOT NULL,
	_fault_uv_ nvarchar(50) NOT NULL,
	_fault_OL_ nvarchar(50) NOT NULL,
	_fault_UL_ nvarchar(50) NOT NULL,
	_fault_OF_ nvarchar(50) NOT NULL,
	_fault_UF_ nvarchar(50) NOT NULL,
	_fault_OT_ nvarchar(50) NOT NULL,
	_fault_GF_ nvarchar(50) NOT NULL,
	_fault_PD_ nvarchar(50) NOT NULL,
	_fault_PU_ nvarchar(50) NOT NULL,
	_fault_ZV_ nvarchar(50) NOT NULL,
	_fault_NV_ nvarchar(50) NOT NULL,
	_n_ nvarchar(50) NOT NULL,
	_s1_ nvarchar(50) NOT NULL,
	_s2_ nvarchar(50) NOT NULL,
	_s3_ nvarchar(50) NOT NULL,
	_s4_ nvarchar(50) NOT NULL,
	_boost_ nvarchar(50) NOT NULL,
	_IsAlertProcessed_ nvarchar(50) NOT NULL,
 CONSTRAINT PK_slcevents PRIMARY KEY 
(
	_id_ ASC
) 
);


CREATE TABLE SLCFaults(
	SLCFaultid int NOT NULL,
	SLCFaultName nvarchar(50) NOT NULL,
	SLCFaultDescription nvarchar(50) NOT NULL,
	SLCFaultSeverity nvarchar(50) NOT NULL,
	Other int NOT NULL,
	gf int NOT NULL,
	Phase_1 int NOT NULL,
	Phase_3 int NOT NULL,
 CONSTRAINT PK_SlcfaultDetails-1 PRIMARY KEY 
(
	SLCFaultid ASC
) 
);


CREATE TABLE SLCFaultsValues(
	slcFaultValuesId int NOT NULL,
	slcFaultId int NOT NULL,
	Value nvarchar(50) NOT NULL,
	Description nvarchar(50) NOT NULL,
 CONSTRAINT PK_SLCFaultsValues PRIMARY KEY 
(
	slcFaultValuesId ASC
) 
);


CREATE TABLE smsauthority(
	id int AUTO_INCREMENT NOT NULL,
	cityid int NULL,
	issend Tinyint NULL,
	smsGatewayID int NULL,
PRIMARY KEY 
(
	id ASC
) 
);

CREATE TABLE smsenthistory(
	deviceid int NULL,
	sentdate date NULL,
	senttime varchar(15) NULL,
	statusid int NULL
);


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


CREATE TABLE statusmaster(
	statusid int AUTO_INCREMENT NOT NULL,
	statusname varchar(500) NULL,
PRIMARY KEY 
(
	statusid ASC
) 
);

ALTER TABLE smsauthority ADD  DEFAULT (NULL) FOR issend;
ALTER TABLE smsenthistory ADD  DEFAULT (NULL) FOR deviceid;
ALTER TABLE smsenthistory ADD  DEFAULT (NULL) FOR sentdate;
ALTER TABLE smsenthistory ADD  DEFAULT (NULL) FOR senttime;
ALTER TABLE smsenthistory ADD  DEFAULT (NULL) FOR statusid;
ALTER TABLE statusmaster ADD  DEFAULT (NULL) FOR statusname;
