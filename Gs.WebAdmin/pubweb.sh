#!/bin/bash
echo ">>进入打包项目路径"
cd /Users/topbrids/Desktop/lfex/lfexApi/src/lfexWeb
echo ">>查看当前路径下文件"
ls
echo ">>执行打包发布命令"
dotnet publish -c release -o ../../release/lfexwebr
echo ">>进入到打包好的路径查看"
cd ../../release/
ls
echo ">>打成压缩包"
tar -zcvf lfexwebr.tar.gz lfexwebr
echo ">>证书上传到服务器 :"
scp -P 6001 -i /Users/topbrids/cert/LFEX.pem lfexwebr.tar.gz root@49.233.134.249:/apps/project/lfex
echo ">>证书远端连接服务器:"
ssh -p 6001 -i /Users/topbrids/cert/LFEX.pem root@49.233.134.249

# echo ">>进入到服务器项目发布路径"
# cd /apps/project/lfex

# echo ">>解压压缩包到当前路径"
# tar -zxvf lfexweb.tar.gz

