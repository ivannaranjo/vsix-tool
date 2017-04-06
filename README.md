# The vsix tool
The `vsix` tool allows the user to install .vsix packages on the experimental instance for Visual Studio. This tool
accepts the following parameters:

```
Usage: vsix [options]

Options:
  -?|-h|--help  Show help information
  -f|--vsix     The path to the vsix to install
  -v|--version  The version of VS to install to.
  -s|--suffix   The Visual Studio root suffix to use.
```

The `--suffix` parameter will be rarely used, as the experimental instance is always "exp". The
 `--version` will default to "14.0" (Visual Studio 2015).

## Why the vsix tool
This tool is necessary because of [this issue](https://github.com/Microsoft/extendvs/issues/41) in which the 
.vsix fails to be extracted due to the paths being too long.

It was inspired by https://github.com/SLaks/Root-VSIX.


