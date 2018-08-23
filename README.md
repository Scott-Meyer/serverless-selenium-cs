# serverless-selenium-cs

This project allows you to test and create selenium c# AWSLambda projects.


# howto
Requires: linux (windows linux subsystem) with dotnet and maketools. Docker.

Clone repository
browse to repository
```bash
make fetch-dependencies
make dotnet
make docker-build
make docker-run
make build-lambda-package
```
Note: if you know how to make it not re-download the bin/ packages if they are already there... please help.

note2: If you are on windows, just exit the bash for the docker parts, and manually paste the lines into cmd.

# uploading
If you have not already, run `make build-lambda-package` for a build.zip

Create new .netcore 2.1 lambda function and upload zip file.

# running
I ran this using the AWSSDK.net and passing a json payload with the 3 required fields which are 'url','height','width'.
