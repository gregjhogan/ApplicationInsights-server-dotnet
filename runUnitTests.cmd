@echo off
@setlocal enabledelayedexpansion enableextensions

CALL buildRelease.cmd

set BuildRoot=%~dp0..\bin\release\

CALL "%VS140COMNTOOLS%..\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" /UseVsixExtensions:true "%BuildRoot%Src\PerformanceCollector\Tests\Unit.Tests.dll" "%BuildRoot%Src\PerformanceCollector\Tests\Xdt.Tests.dll" "%BuildRoot%Src\DependencyCollector\Net40.Tests\Microsoft.ApplicationInsights.DependencyCollector.Net40.Tests.dll" "%BuildRoot%Src\DependencyCollector\Net40.Tests\Microsoft.ApplicationInsights.TestFramework.Net40.dll" "%BuildRoot%Src\DependencyCollector\Net45.Tests\Microsoft.ApplicationInsights.DependencyCollector.Net45.Tests.dll" "%BuildRoot%Src\DependencyCollector\Net45.Tests\Microsoft.ApplicationInsights.TestFramework.Net45.dll" "%BuildRoot%Src\DependencyCollector\Nuget.Tests\Microsoft.ApplicationInsights.DependencyCollector.NuGet.Tests.dll" "%BuildRoot%Src\Web\Web.Net40.Tests\Microsoft.ApplicationInsights.Web.Net40.Tests.dll" "%BuildRoot%Src\Web\Web.Net45.Tests\Microsoft.ApplicationInsights.Web.Net45.Tests.dll" "%BuildRoot%Src\Web\Web.Nuget.Tests\Microsoft.ApplicationInsights.Platform.Web.Nuget.Tests.dll" "%BuildRoot%Src\WindowsServer\WindowsServer.Net40.Tests\Microsoft.ApplicationInsights.TestFramework.Net40.dll" "%BuildRoot%Src\WindowsServer\WindowsServer.Net40.Tests\WindowsServer.Net40.Tests.dll" "%BuildRoot%Src\WindowsServer\WindowsServer.Net45.Tests\Microsoft.ApplicationInsights.TestFramework.Net45.dll" "%BuildRoot%Src\WindowsServer\WindowsServer.Net45.Tests\WindowsServer.Net45.Tests.dll" "%BuildRoot%Src\WindowsServer\WindowsServer.Nuget.Tests\WindowsServer.Nuget.Tests.dll" /logger:trx

