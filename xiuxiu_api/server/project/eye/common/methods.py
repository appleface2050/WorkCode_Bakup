# -*- coding: utf-8 -*-


class Methods(object):
    @staticmethod
    def get_one_object_info_from_scratch(objects, get_object_info, count, loaded_ids, *params):
        result = list()
        index = 0
        for obj in objects:
            if index < count:
                if str(obj.id) in loaded_ids:
                    continue
                else:
                    temp = get_object_info(obj, *params)
                    if temp:
                        result.append(temp)
                        index += 1
            else:
                break
        return result

    @staticmethod
    def get_one_object_info_not_from_scratch(objects, get_object_info, start_id, count, loaded_ids, *params):
        result = list()
        object_ids = [obj.id for obj in objects]
        if start_id in object_ids:
            the_index = object_ids.index(start_id)
            the_objects = list(objects)[the_index:]
            result = Methods.get_one_object_info_from_scratch(
                the_objects, get_object_info, count, loaded_ids, *params)

        return result

    @staticmethod
    def increment_unread_win_or_lost(obj, is_win, is_change):
        if is_change:
            if is_win:
                obj.win_count += 1
                obj.lost_count -= 1
            else:
                obj.win_count -= 1
                obj.lost_count += 1
        else:
            if is_win:
                obj.win_count += 1
            else:
                obj.lost_count += 1
        obj.save()
