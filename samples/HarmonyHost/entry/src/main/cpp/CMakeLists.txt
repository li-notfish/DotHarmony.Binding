cmake_minimum_required(VERSION 3.5.0)
project(harmony_host C)

add_library(entry SHARED napi_init.c)

find_library(hilog-lib hilog_ndk.z)
find_library(ace-napi-lib ace_napi.z)

target_link_libraries(entry PUBLIC ${hilog-lib} ${ace-napi-lib})
