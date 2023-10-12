#!/bin/bash
echo ">>进入打包项目路径"
cd /Users/topbrids/Desktop/lfex/lfexApi/src/lfexApi
echo ">>查看当前路径下文件"
ls
echo ">>执行打包发布命令"
dotnet publish -c release -o /Users/topbrids/Desktop/lfex/lfexApi/release/lfexapi
echo ">>进入到打包好的路径查看"
cd /Users/topbrids/Desktop/lfex/lfexApi/release/
ls
echo ">>打成压缩包"
tar -zcvf lfexapi.tar.gz lfexapi
echo ">>上传到服务器 请输入密码:"
scp -i /Users/topbrids/cert/LFEX.pem -P 6001 lfexapi.tar.gz root@49.233.134.249:/apps/project/lfex
echo ">>证书远端连接服务器 请输入密码:"
ssh -p 6001 -i /Users/topbrids/cert/LFEX.pem root@49.233.134.249



# echo ">>进入到服务器项目发布路径"
# cd /apps/project/lfex

# echo ">>查看当前路径"
# ls

# echo ">>解压压缩包到当前路径"
# tar -zxvf lfexapi.tar.gz

# echo ">>进入supeervisorctl查看状态"
# supervisorctl

#echo ">>重启lfexdevapi 服务"

