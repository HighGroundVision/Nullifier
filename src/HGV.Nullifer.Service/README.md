Services are Dumd, but they work. So these are the steps needed to run the service.
1. Create Database 'DotaAbilityDraftStats' in SQL Server
2. Populate 'User.App.config' with STEAM API key
3. Run 'installutil HGV.Nullifer.Service.exe' against Release
4. Change LogIn details to a windows user with access to SQL SERVER
5. Start service
6. Check event log for infomation about startup / shutdown / internal audit