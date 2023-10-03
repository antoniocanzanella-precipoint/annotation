CREATE TABLE geometries
(
    name varchar,
    geom geometry
);

INSERT INTO geometries
VALUES ('Point', 'POINT(0 0)'),
       ('Linestring', 'LINESTRING(0 0, 1 1, 2 1, 2 2)'),
       ('Polygon', 'POLYGON((0 0, 1 0, 1 1, 0 1, 0 0))'),
       ('PolygonWithHole', 'POLYGON((0 0, 10 0, 10 10, 0 10, 0 0),(1 1, 1 2, 2 2, 2 1, 1 1))'),
       ('Collection', 'GEOMETRYCOLLECTION(POINT(2 0),POLYGON((0 0, 1 0, 1 1, 0 1, 0 0)))');

SELECT name, ST_AsText(geom)
FROM geometries;

SELECT name, ST_AsGeoJSON(geom)
FROM geometries;

SELECT name, ST_AsGeoJSON(geom)
FROM geometries;

SELECT ST_AsGeoJSON(tab.*)
FROM geometries as tab;

select *
from ims."Annotations";
select *
from ims."CounterGroups";
select *
from ims."Counters";
select *
from ims."ClassGroup";
select *
from ims."ClassGroupsForAnnotations";

select count(*), 'Annotations'
from ims."Annotations"
union all
select count(*), 'Counters Group'
from ims."CounterGroups"
union all
select count(*), 'Counters'
from ims."Counters";

delete
from ims."Annotations";
delete
from ims."CounterGroups";
delete
from ims."Counters";
delete
from ims."Imports";
delete
from ims."Folders";


INSERT INTO ims."Annotations"
("Id", "Label", "Description", "IsVisible", "Confidence", "Shape", "SlideImageId", "Type",
 "Visibility", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate")
VALUES ('c41c195f-1948-4e2e-b807-e8651d4ba4e7', 'Mattew''s Point', 'Mattew''s Point description', true, 0,
        'POLYGON((0 0, 0 500, 500 500, 500 0, 0 0))', '3bbcb0bd-f692-4ef3-804e-154f270c829f', 'rectangular',
        'private', '27c43166-b25b-4c28-8322-2634a3fdbbc0', '2021-07-13 11:30:55.057', null, null);

INSERT INTO ims."CounterGroups"
("Id", "Label", "Description", "AnnotationId", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate")
VALUES ('654886b7-ec45-48be-a1af-5acf5892a61d', 'Mattew''s counting group', 'Mattew''s counting group',
        'c41c195f-1948-4e2e-b807-e8651d4ba4e7', '27c43166-b25b-4c28-8322-2634a3fdbbc0', '2021-07-13 11:30:55.057', null,
        null);

INSERT INTO ims."Counters"
    ("Id", "Shape", "GroupCounterId")
VALUES ('b0175e5f-3794-4011-9533-730cba53b523', 'POINT(200 200)', '654886b7-ec45-48be-a1af-5acf5892a61d');
VALUES ('3911be12-2ab0-4ed5-aedb-30a0db96bcef', 'POINT(300 300)', '654886b7-ec45-48be-a1af-5acf5892a61d');


UPDATE ims."Annotations" as annota
SET "Shape"=st_translate(annota."Shape", 10, 10)
WHERE "Id" = 'c41c195f-1948-4e2e-b807-e8651d4ba4e7';

select a."Shape"
from ims."Annotations" a

UPDATE ims."Annotations" as ant
SET ant."Shape"=st_translate("Shape", 10, 10)
WHERE ant."Id" = 'c41c195f-1948-4e2e-b807-e8651d4ba4e7'
  and (ant."CreatedBy" = '27c43166-b25b-4c28-8322-2634a3fdbbc0' or ant."Visibility" = 'public');

update ims."Counters" as c
set c."Shape" = st_translate(c."Shape", 10, 10) from ims."Counters" as c join ims."CounterGroups" as cg
on cg."Id" = c."GroupCounterId" join ims."Annotations" as annota on cg."AnnotationId" = annota."Id"
where annota."Id" = 'c41c195f-1948-4e2e-b807-e8651d4ba4e7'
  and (annota."CreatedBy" = '27c43166-b25b-4c28-8322-2634a3fdbbc0'
   or annota."Visibility" = 'public');

UPDATE ims."Counters"
SET ims."Counters"."Shape" = st_translate(ims."Counters"."Shape", 10, 10) from ims."CounterGroups" as cg, ims."Annotations" as annota
where cg."Id" = ims."Counters"."GroupCounterId"
  and cg."AnnotationId" = annota."Id"
  and annota."Id" = 'c41c195f-1948-4e2e-b807-e8651d4ba4e7'
  and (annota."CreatedBy" = '27c43166-b25b-4c28-8322-2634a3fdbbc0'
   or annota."Visibility" = 'public');

UPDATE ims."Counters"
SET ims."Counters"."Shape" = st_translate(ims."Counters"."Shape", 10, 10);


select *
from ims."Folders" f WITH recursive folders ("Id", "Name", "ParentFolderId", FolderBelow)
AS (
SELECT f."Id", f."Name", f."ParentFolderId", 0
FROM ims."Folders" f
WHERE f."Id" = '10329e4e-9b1c-4054-9985-0454d4fa5ade'
UNION ALL
SELECT e."Id", e."Name", e."ParentFolderId", o.FolderBelow + 1
FROM ims."Folders" e
INNER JOIN folders o
ON o."Id" = e."ParentFolderId"
)
SELECT *
FROM folders WITH recursive folders ("Id", FolderBelow) AS (
SELECT f."Id", 0 FROM ims."Folders" f
UNION ALL
SELECT e."Id", o.FolderBelow + 1 FROM ims."Folders" e INNER JOIN folders o ON o."Id" = e."ParentFolderId"
)
select *
FROM folders;


SELECT f.Id, f.Name, f.BriefDescription, f.Description, f.ParentFolderId, 0
FROM ims."Folders" f
UNION ALL
SELECT e.Id, e.Name, e.BriefDescription, e.Description, e.ParentFolderId, o.FolderBelow + 1
FROM ims.Folders e
         INNER JOIN ims.Folders o ON o.Id = e.ParentFolderId



ALTER TYPE annotation_visibility ADD VALUE 'read_only' AFTER 'public';

select count(*)
from annotation.ims."Annotations" a;

select count(a."Id"), a."Type"
from annotation.ims."Annotations" a
group by a."Type";
select count(a."Id"), a."SlideImageId"
from annotation.ims."Annotations" a
group by a."SlideImageId";
