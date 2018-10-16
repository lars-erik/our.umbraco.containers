param(
	$where = ""
)

if ($where -eq "") {
	&.\packages\NUnit.ConsoleRunner.3.9.0\tools\nunit3-console.exe .\Our.Umbraco.Containers.Castle.UmbracoTests\bin\debug\Umbraco.Tests.dll
} else {
	&.\packages\NUnit.ConsoleRunner.3.9.0\tools\nunit3-console.exe .\Our.Umbraco.Containers.Castle.UmbracoTests\bin\debug\Umbraco.Tests.dll `
		--where $where
}