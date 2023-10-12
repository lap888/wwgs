echo ">>进入到服务器项目发布路径"
cd /apps/project

echo ">>查看当前路径"
ls

echo ">>解压压缩包到当前路径"
tar -zxvf xzxapidev.tar.gz

echo ">>进入supeervisorctl查看状态"
supervisorctl

#echo ">>重启lfexdevapi 服务"
#restart devlfexapi
