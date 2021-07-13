

create table AppScanResult (
	ID int identity(1,1) not null,
	DateScanned datetime not null default getdate(),
	Title varchar(100) not null default '',
	StudentID varchar(100) not null default '',
	StudentNo varchar(100) not null default '',
	ComponentName varchar(100) not null default '',
	DateCreated datetime not null default getdate()
 )


 sc.exe create fileuploader binPath= "<path_to_the_service_executable>" start=auto
 sc.exe create servicetest binPath= "C:\servicetest\dist\servicetest.exe" start=demand


