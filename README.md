# KMOU_OceanCluster_UnityProject
KMOU OceanCluster 2024, unity

## git 명령어 모음
```
git checkout -b feature/[branch-name]
git add .
git commit -m "[작업 내용 설명]"
git push origin feature/[branch-name]
```
### LFS
```
git lfs track "*.psd"
git lfs migrate import --include="*.psd"
git lfs track "*.dll"
git lfs track "*.dylib"
git lfs track "*.so"
git lfs track "*.db"
cat .gitattributes
git lfs migrate import --include="*.dll,*.dylib,*.so,*.db"
git add .
git commit -m "Migrate large files to LFS"
git push origin feature/[branch-name]
```
