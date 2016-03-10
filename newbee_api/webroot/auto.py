#coding=utf8
import os
import re

webroot = os.getcwd()

#domain = 'api.sangebaba.com'

domain = 'api.release.sangebaba.com'

#desdir = 'api-doc-line'

desdir = 'api-doc-release'

source = webroot + '/api-doc-local'

destination = webroot + '/' + desdir





def readfile(dir):
	yid = os.walk(dir)
	for rootDir, pathList, fileList in yid:
		for file in fileList:
                        original = os.path.join(rootDir, file)
                        des = os.path.join(destination, file)
                        replacedomain(original, des, file)
			
		

def replacedomain(source, des, filename):
	fs = open(source, 'r')
	fd = open(des, 'w')
	contentSrc = fs.read()
	if filename == 'main.json' or filename == 'upgrademain.json':
		replace = '	"basePath": "http://' + domain + '/' + desdir
		contentDes = re.sub(r'.*"basePath"\s?:\s*"http://.*/api-doc-local', replace, contentSrc)
	else:
		replace = '    "basePath": "http://' + domain + '/'
		contentDes = re.sub(r'.*"basePath"\s?:\s*"http://.*/', replace, contentSrc)
		
	fd.write(contentDes);
	fs.close();
	fd.close();

readfile(source)
