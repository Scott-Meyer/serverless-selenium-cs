FROM lambci/lambda:dotnetcore2.1
MAINTAINER scott.r.meyer@gmail.com

USER root

ENV APP_DIR /var/task

WORKDIR $APP_DIR