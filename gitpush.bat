git add .
set /p message=Enter a message: 
set /p version=What is the version? 
git commit -m "V%version% Changes: %message%"
git push origin master

echo "Pushed %version% with Message: %message%"