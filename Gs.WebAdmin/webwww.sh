echo "www"
ssh -i /Users/topbrids/cert/testbbs.pem root@101.32.178.79


scp -i /Users/topbrids/cert/testbbs.pem /Users/topbrids/Downloads/index.html root@101.32.178.79:/apps/www/lfex



scp -i /Users/topbrids/cert/testbbs.pem /Users/topbrids/wwgs/Gs.WebAdmin/mbm/index.html root@101.32.178.79:/apps/www/mbm


scp -i /Users/topbrids/cert/testbbs.pem root@101.32.178.79:/apps/www/mbm ./