git add .
set /p message=Enter a message
git commit -m %message%
git push origin master