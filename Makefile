clean:
	rm -rf build build.zip

fetch-dependencies:
	mkdir -p bin/

	# Get chromedriver
	curl -SL https://chromedriver.storage.googleapis.com/2.35/chromedriver_linux64.zip > chromedriver.zip
	unzip chromedriver.zip -d bin/

	# Get Headless-chrome
	curl -SL https://github.com/adieuadieu/serverless-chrome/releases/download/v1.0.0-29/stable-headless-chromium-amazonlinux-2017-03.zip > headless-chromium.zip
	unzip headless-chromium.zip -d bin/

	# Clean
	rm headless-chromium.zip chromedriver.zip


dotnet:
	cd src; dotnet publish -c Release -o pub
	cp -R bin src/pub/

docker-build:
	docker-compose build

docker-run:
	docker-compose run lambda serverless-selenium-cs::serverless_selenium_cs.Function::FunctionHandler "{\"url\": \"value3\",\"width\": \"value2\",\"height\": \"value1\"}"

build-lambda-package: clean fetch-dependencies dotnet
	mkdir build
	cp -r src/pub/* build/.
	cp -r bin build./
	cd build; zip -9qr build.zip .
	cp build/build.zip .
	rm -rf build