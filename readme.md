Super alpha project

For now implements a castle adapter for Umbraco v8 provided PR 2690 is merged.

To run all Umbraco's tests with Castle as the container, use powershell to execute
.\umbraco-tests-castle.ps1

References Umbraco v8 on my local disk. To build, make reference paths in your
project user settings.

TODO:
- Verify behavior of Castle when naming all services to be able to
reinstate two intermediate service registrations in said PR
- Verify more stuff in both PR and this project
- More containers?