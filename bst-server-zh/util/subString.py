# coding=utf-8

def subString(string,length):
    if length >= len(string):
        return string
    result = ''
    i = 0
    p = 0
    while True:
        ch = ord(string[i])
        #1111110x
        if ch >= 252:
            p = p + 6
        #111110xx
        elif ch >= 248:
            p = p + 5
        #11110xxx
        elif ch >= 240:
            p = p + 4
        #1110xxxx
        elif ch >= 224:
            p = p + 3
        #110xxxxx
        elif ch >= 192:
            p = p + 2
        else:
            p = p + 1

        if p >= length:
            break;
        else:
            i = p
        return string[0:i]