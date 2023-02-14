#For Mac
./uas auth login --uos_app_id [App ID] --uos_app_secret [App Secret]
# Sync Bundles to uos Bucket
./uas entries sync ./Bundles Bundles --bucket [Bucket ID]
# Sync UpdateInfo to uos Bucket
./uas entries copy ./BundlesCache/OSX/updateinfo.json Bundles/OSX/updateinfo.json --bucket [Bucket ID]

# # For Windows
# uas auth login --uos_app_id [App ID] --uos_app_secret [App Secret]
# # Sync Bundles to uos Bucket
# uas entries sync "E:\unity project\Bundles" Bundles --bucket [Bucket ID]
# # Sync UpdateInfo to uos Bucket
# uas entries copy "E:\unity project\BundlesCache\Windows\updateinfo.json" Bundles/Windows/updateinfo.json --bucket [Bucket ID]