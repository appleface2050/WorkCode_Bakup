# coding=utf-8
try:
    import hashlib
    md5_constructor = hashlib.md5
    md5_hmac = md5_constructor
    sha_constructor = hashlib.sha1
    sha_hmac = sha_constructor
except ImportError:
    import md5
    md5_constructor = md5.new
    md5_hmac = md5
    import sha
    sha_constructor = sha.new
    sha_hmac = sha

def cr_lottery_check_token(token, guid, prize):
    if token == md5_constructor(guid+prize).hexdigest():
        return True
    else:
        return False