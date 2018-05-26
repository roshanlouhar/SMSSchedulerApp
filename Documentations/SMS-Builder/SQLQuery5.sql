 --update [dbo].[slcevents] set _IsAlertProcessed_ = '0' where _id_ =1002; 

 -- update [dbo].[cp] set _mobile_ ='8003644328';

 --select top 1  * from [dbo].[slcevents] where _deviceid_ ='3720' and _id_ < 29554 order by _dt_ desc

  --select _deviceid_ ,count(*) from [dbo].[slcevents] group by _deviceid_

 
 --select * from  [dbo].[slc_devices] where _deviceid_ =1005 ;

 -- select count(*) as Issend from [dbo].[smsauthority] where cityid =9 and issend =1 ;

 --select * from [dbo].[SLCFaults] where SLCFaultName = 'r'

 --select * from cp where _deviceid_ =1005 ;

--INSERT INTO [dbo].[smsenthistory] ([deviceid],[sentdate],[senttime],[statusid])VALUES(1005,'2018-05-21',' 19:45:00', '1')
           
--  select sg.* from [dbo].[smsauthority] sa inner join  [dbo].[SMSGateway] sg on sa.smsGatewayID =sg.SMSGatewayID where sa.cityid =9;


--slcevents columns
--_id_	_slcid_	_deviceid_	_dt_	_aux1_	_aux2_	_aux3_	_aux4_	_aux5_	_aux6_	_aux7_	_aux8_	
--_r_failure_ --_y_failure_	--_b_failure_--	_r_	--	_y_	--	_b_	--_fault_ov_	_fault_uv_	_fault_OL_	_fault_UL_	
--_fault_OF_	_fault_UF_	_fault_OT_	_fault_GF_	_fault_PD_	_fault_PU_  _fault_ZV_	_fault_NV_	
--_n_	_s1_	_s2_	_s3_	_s4_	_boost_	
--_IsAlertProcessed_


--slc_devices
--_deviceid_	_dt_	_name_	_switchno_	_Location_	_area_	_rating_	_totalload_	_totalload_b_	
--_totalload_y_	_numberofpoles_	_numberofpoles_b_	_numberofpoles_y_	_referencewattage_b_	_referencewattage_y_
--_referencewattage_	_typeofload_	_latitude_	_longitude_	_mobile_	_city_	_userid_	_phase_	_wardno_	_zone_
--commandmode_	_updated_	_cpname_	_cpmobileno_


--slcFaultzinformation
--SLCFaultid	SLCFaultName	SLCFaultDescription	SLCFaultSeverity

--SMSGatewayID	SMSGatewayURL	SMSGatewayUserid	SMSGatewayPwd

--select top 1 sfv.description ,sf.SLCFaultSeverity  from  [SLCFaults]  sf inner join SLCFaultsValues sfv on sf.SLCFaultid =sfv.SLCFaultid
--where ((sf.SLCFaultName ='r' and sfv.value = '0') or (sf.SLCFaultName ='y' and sfv.value = '0') or (sf.SLCFaultName ='b' and sfv.value = '0') )

--select se.* from slcevents se  inner join slc_devices sd on se._deviceid_ = sd._deviceid_  where se._IsAlertProcessed_ = 0 and  sd._city_ in (9,15,16) ;

