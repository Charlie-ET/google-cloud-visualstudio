[![Build status](https://ci.appveyor.com/api/projects/status/0s0wo324dmv18uo9/branch/master?svg=true)](https://ci.appveyor.com/project/ivannaranjo/google-cloud-visualstudio-bnnrp/branch/master)

# Visual Studio Extension for Google Cloud Platform

This repo contains the source code for the Visual Studio Extension for Google
Cloud Platform.

To build and install the extension you will need to have Visual Studio 2015
installed with the Visual Studio SDK feature installed. After that just open the
.sln file and build it, that is really it. If you get an error during the
restoration of the NuGet packages ensure that you have enabled nuget.org as a
source in the NuGet Package Manager.

In order to enable Windows VM password reset feature the Google Cloud SDK needs
to be installed, which you can do from
[here](https://cloud.google.com/sdk/). The reset Windows VM password depends on
the `reset-windows-password` command to be installed. To install this command
install the `beta` command group as follows:
```bash
gcloud components install beta
```

## Builds
We use appveyor to build on every push to this branch, the latest release build of the extension for the master branch can be found [here](https://ci.appveyor.com/api/projects/ivannaranjo/google-cloud-visualstudio/artifacts/GoogleCloudExtension/GoogleCloudExtension/bin/Release/GoogleCloudExtension.vsix?branch=master).

## Support

To get help on using the Visual Studio Extension, please log an issue with this
project. While we will eventually be able to offer support on Stack Overflow or
a Google+ community, for now your best bet is to report issues in this Github
project.

Patches are encouraged, and may be submitted by forking this project and
submitting a Pull Request. See [CONTRIBUTING.md](CONTRIBUTING.md) for more
information.

## License

Apache 2.0. See [LICENSE](LICENSE) for more information.
