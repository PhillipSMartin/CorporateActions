@echo off
IF DEFINED PROGRAMFILES(x86) (
  SET "_PROGRAMFILES=%PROGRAMFILES(x86)%"
) ELSE (
  SET "_PROGRAMFILES=%PROGRAMFILES%"
)
%echo on

CD "%_PROGRAMFILES%\Gargoyle Strategic Investments\CorporateActionsImport"

"%_PROGRAMFILES%\Gargoyle Strategic Investments\CorporateActionsImport\CorporateActionsImport.exe" /r:TWS /ip:gargoyle-mw20 /tname:CorporateActionsUpdate /numdays:7

