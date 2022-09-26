(!) IMPORTANT
Extra packages & their dependencies


-- Utilities.unitypackage --
Contains assets that extend the OSA's capabilities, 
but also general purpose utilities that can be used outside OSA
DEPENDENCIES: none

-- Demos.unitypackage --
Contains all the example scenes and their associated assets. 
It also includes everything from TableView.unitypackage (exact duplicates), 
because the table_view scene inside Demos depends on it
DEPENDENCIES: Utilities.unitypackage, so import Utilities.unitypackage first.

-- TableView.unitypackage -- 
Contains assets for implementing a TableView, manually or through OSA Wizard.
DEPENDENCIES: none

-- TMPro/TableViewTMProSupport.unitypackage -- 
DEPENDENCIES: TableView.unitypackage, so import TableView.unitypackage first.
See TMPro/TMPro instructions.txt


*On some Unity versions, you need to move the .unitypackage files outside the 
project, go back to Unity to see they disappeared, then import them from the
new folder 