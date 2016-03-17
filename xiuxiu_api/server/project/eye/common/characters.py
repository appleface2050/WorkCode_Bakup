# -*- coding: utf-8 -*-

import chardet
from logs import Logs


class Characters(object):
    @staticmethod
    def unicode_to_concrete(content, concrete="utf8"):
        result = ""
        if isinstance(content, str):
            encoding_dict = chardet.detect(content)
            try:
                if encoding_dict["encoding"] is None:
                    result = content
                else:
                    result = content.decode(encoding_dict["encoding"]).encode(concrete)
            except Exception as ex:
                Logs.print_current_function_name_and_line_number(ex)
                result = ""
        elif isinstance(content, unicode):
            result = content.encode(concrete)
        elif isinstance(content, list):
            result = [Characters.unicode_to_concrete(c, concrete) for c in content]
        elif isinstance(content, tuple):
            result = [Characters.unicode_to_concrete(c, concrete) for c in content]
            result = tuple(result)
        elif isinstance(content, dict):
            result = {Characters.unicode_to_concrete(key, concrete): Characters.unicode_to_concrete(value, concrete)
                      for key, value in content.items()}
        return result
