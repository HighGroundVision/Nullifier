Services are silly and require so much setup to run, and do not get be started on debuging. But, they work, so these are the steps needed to run the service.
1. Create Database 'DotaAbilityDraftStats' in SQL Server
2. Populate 'User.App.config' with STEAM API key
3. Run 'installutil HGV.Nullifer.Service.exe' against Release in Developer Command Prompt for VS 2017 as Admin
4. Change LogIn details to a windows user with access to SQL SERVER
5. Start service
6. Check event log for infomation about startup / shutdown / internal audit