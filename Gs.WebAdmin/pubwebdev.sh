#!/bin/bash
echo ">>进入打包项目路径"
cd /Users/topbrids/wwgs/Gs.WebAdmin
echo ">>查看当前路径下文件"
ls
echo ">>执行打包发布命令"
dotnet publish -c release -o /Users/topbrids/wwgs/release/xzxadmin
echo ">>进入到打包好的路径查看"
cd /Users/topbrids/wwgs/release/
ls
echo ">>打成压缩包"
tar -zcvf xzxadmin.tar.gz xzxadmin

echo ">>上传到服务器 请输入密码:"
scp xzxadmin.tar.gz root@129.28.186.13:/apps/project/xzxadmin

echo ">>密码远端连接服务器 请输入密码:"
ssh root@129.28.186.13

# echo ">>进入到服务器项目发布路径"
# cd /apps/project/lfex

# echo ">>解压压缩包到当前路径"
# tar -zxvf lfexapi_dev.tar.gz


# echo "www"
# ssh -i /Users/topbrids/cert/testbbs.pem root@101.32.178.79


# scp -i /Users/topbrids/cert/testbbs.pem /Users/topbrids/Downloads/index.html root@101.32.178.79:/apps/www/lfex



# scp -i /Users/topbrids/cert/testbbs.pem /Users/topbrids/Desktop/mbm/index.html root@101.32.178.79:/apps/www/mbm


# scp -i /Users/topbrids/cert/testbbs.pem root@101.32.178.79:/apps/www/mbm ./
