clean:
	rm -rf build build.zip

fetch-dependencies:
	mkdir -p bin/

	# Get chromedriver
	curl -SL https://chromedriver.storage.googleapis.com/2.32/chromedriver_linux64.zip > chromedriver.zip
	unzip chromedriver.zip -d bin/

	# Get Headless-chrome
	curl -SL https://github.com/adieuadieu/serverless-chrome/releases/download/v1.0.0-29/stable-headless-chromium-amazonlinux-2017-03.zip > headless-chromium.zip
	unzip headless-chromium.zip -d bin/

	# Clean
	rm headless-chromium.zip chromedriver.zip

dotnet:
	dotnet publish -c Release -o pub

docker-build:
	docker-compose build

docker-run:
	docker-compose run lambda serverless-selenium-cs::serverless_selenium_cs.Function::FunctionHandler


#docker run --rm -v "c:/users/scott meyer/documents/git/serverless-selenium-cs/src/pub":/var/task lambci/lambda:dotnetcore2.1 serverless-selenium-cs::serverless_selenium_cs.Function::FunctionHandler '{"some": "event"}'


#docker run --rm -v "c:/users/scott meyer/documents/git/serverless-selenium-cs/src/pub":/var/task serverless-selenium-cs_lambda serverless-selenium-cs::serverless_selenium_cs.Function::FunctionHandler '{"some": "event"}'