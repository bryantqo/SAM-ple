UPDATE pam_1_0_0.groupacl
	SET acl = acl||'[ "Instrument:UploadShapefile" ]'	
WHERE groupkey = 'Admin';


UPDATE pam_1_0_0.groupacl
	SET acl = acl||'[ "Instrument:UploadShapefile" ]'	
WHERE groupkey = 'Lands_Staff';