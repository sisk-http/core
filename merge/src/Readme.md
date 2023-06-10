To build this file, you can use bflat:

```shell
$ git clone https://github.com/sisk-http/core.git
$ cd .\core\merge\src
$ bflat build -o merge.exe
```

And then, compile your self-contained source code to

```
$ .\merge.exe .\core\src
```