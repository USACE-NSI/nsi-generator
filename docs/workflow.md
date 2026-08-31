# NSI Generator Workflow

The NSI Generator processes standard data structures at the major processes in this
workflow. Raw base data is highly variable, so each step that touches it mutates it
into one of the standard structures defined in [datasets.md](datasets.md). This
document covers the workflow steps only; the overall diagram is the **WorkflowSteps**
page of [ExampleDatasets.drawio](../drawio/ExampleDatasets.drawio).

## Inputs

| Input         | Consumed by                                    |
| ------------- | ---------------------------------------------- |
| Bing          | Create Consolidated Footprint Dataset          |
| USAStructures | Create Consolidated Footprint Dataset          |
| 133 Cities    | Create Consolidated Footprint Dataset          |
| Census        | Reconcile Missing Footprints, Distribute Population |
| LEHD          | Distribute Population                          |
| HIFLD Inputs  | FortifyParcelData                              |
| Parcels       | FortifyParcelData                              |
| Business Data | FortifyParcelData                              |
| FEMA FIRMS    | Floodplains and Year Built Adjustments         |

## Steps

### 1. Create Consolidated Footprint Dataset

- **Inputs:** Bing, USAStructures, 133 Cities (input footprint datasets)
- **Output:** Footprint Dataset (consolidated)

Merges the set of input footprint datasets into a single best-of-breed footprint
dataset. Where multiple datasets cover the same footprint in the same geography,
the priority order of each input dataset determines which one wins. See
[datasets.md — A set of input footprint datasets](datasets.md#a-set-of-input-footprint-datasets).

### 2. Reconcile Missing Footprints

- **Inputs:** consolidated Footprint Dataset, Census
- **Output:** reconciled Footprint Dataset

Uses census expectations to find structures that should exist in a zone but have
no footprint, and fills them in — including assigning square footage and building
heights where the underlying input data is missing.

### 3. FortifyParcelData

- **Inputs:** HIFLD Inputs, Parcels, Business Data
- **Output:** Parcel Dataset (fortified)

Joins the parcel dataset with spatial datasets that represent specialty buildings
(prisons, schools, hospitals, etc.) to create the fortified parcel dataset. The
specialty attributes that describe location and population characteristics are
stored on the parcel as optional fields for later processing. See
[datasets.md — A fortified parcel dataset](datasets.md#a-fortified-parcel-dataset).

### 4. Distribute Population

- **Inputs:** Census, LEHD, Parcel Dataset
- **Output:** Population Dataset

Produces population zones carrying expected population counts (Over65 / Under65)
and incoming/outgoing population sets (Working from LEHD; Students and Teachers
from the specialty building data joined into the fortified parcel dataset). See
[datasets.md — A population dataset](datasets.md#a-population-dataset).

### 5. DropPoints

- **Inputs:** reconciled Footprint Dataset, Parcel Dataset
- **Output:** Point Inventory

Materializes the structure inventory as points: one point per footprint where
footprints exist, and points placed at parcel centroids (or other means of
placement) where they do not. The number of fallback placements anticipated in a
zone is indicated by the StructureZonalStatisticsDataset — expected structure
counts versus structures already accounted for by footprints.

### 6. AssignPopulation

- **Inputs:** Point Inventory, Population Dataset
- **Output:** Populated Point Inventory

Assigns population to each point from the population zone it falls in: the
Over65 / Under65 split, and for structures tied to specialty buildings, students,
teachers, and working population drawn from the zone's outgoing population set.

### 7. Floodplains and Year Built Adjustments

- **Inputs:** Populated Point Inventory, FEMA FIRMS
- **Output:** updated Populated Point Inventory

Adjusts structure attributes using FIRMS data: floodplain exposure and
year-built corrections.

### 8. QA/QC

- **Inputs:** Populated Point Inventory
- **Output:** NSI (final product)

Validates the populated inventory. The validated output is the NSI.

## Open items

- The StructureZonalStatisticsDataset is defined in datasets.md but is not a node
  in the workflow diagram. It is presumably consumed by Reconcile Missing
  Footprints and/or DropPoints — add it to the diagram once the consuming step
  is confirmed.
- "DropPoints" is an opaque name for the step that builds the point inventory;
  consider "Generate Point Inventory".
- The diagram says "Reconsile Missing Footprints" — should read "Reconcile".