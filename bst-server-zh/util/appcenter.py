# coding=utf-8

def screenshots_modify(screenshots):
    """
    规整截屏
    """
    if screenshots:
        try:
            result = screenshots.split(",")
            return result
        except Exception as e:
            return screenshots


def modify_date_modify(modify_date):
    """
    只留日期
    """
    return modify_date[:10]


def app_size_modify(size):
    """
    app大小规整
    """
    type = {
        0:"B",
        1:"KB",
        2:"MB",
        3:"GB",
        4:"TB",
    }
    count = 0
    while size > 1024:
        size /= 1024.
        count += 1
    try:
        res = "%.01f"%size + type[count]
    except Exception as e:
        res = "0MB"
    return res


def cal_total_page(total_num, res_num):
    return 1 + ((total_num-1) / res_num)


def content_chinese_word(query):
    """
    判断是否含有中文
    """
    # if query == "qq":
    #     return True
    import re
    zhPattern = re.compile(u'[\u4e00-\u9fa5]+')
    match = zhPattern.search(query)
    if match:
        return True
    else:
        return False


def handle_tojson_type(type):
    """
    将type改为合适的数据
    """
    type_tmp = []
    if type.find(",") != -1:
        type = type.split(",")
        for i in type:
            if i.find(":") != -1:
                type_tmp.append(i.split(":")[1])
            else:
                type_tmp.append(i)
    elif type.find(":") != -1:
        type_tmp.append(type.split(":")[1])
    else:
        type_tmp.append(type)
    type = type_tmp
    return type