// add inline documentation

/**
 * Vue application instance.
 * @typedef {Object} App
 * @property {boolean} isLoaded - Indicates if the app is loaded.
 * @property {boolean} isDragging - Indicates if an element is being dragged.
 * @property {Object} importObject - The imported object from local storage.
 * @property {string} importText - The imported text.
 * @property {string} search - The search string for block name.
 * @property {string} sizeFilter - The size filter for blocks.
 * @property {Array} selectedBlocks - The array of selected blocks.
 * @property {Array} componentList - The list of components.
 * @property {number} totalVolume - The total volume of components.
 * @property {number} totalWeight - The total weight of components.
 * @property {boolean} visible1 - Indicates if the first element is visible.
 * @property {boolean} visible2 - Indicates if the second element is visible.
 * @property {boolean} visible3 - Indicates if the third element is visible.
 * @property {boolean} visible4 - Indicates if the fourth element is visible.
 * @property {boolean} visibleBlockData - Indicates if the block data is visible.
 * @property {string} selectedBlocksString - The string representation of selected blocks.
 * @property {Array} icons - The array of icons.
 */

const app = Vue.createApp({
  data() {
    return {
      isLoaded: false,
      isDragging: false,
      importObject: localStorage.getItem("importObject") ? JSON.parse(localStorage.getItem("importObject")) : null,
      importText: "",
      search: "", //Search string for block name
      sizeFilter: "large/small",
      selectedBlocks: localStorage.getItem("selectedBlocks") ? JSON.parse(localStorage.getItem("selectedBlocks")) : [], //Array for the blocks that were selected as part of the planner
      componentList: [],
      totalVolume: 0,
      totalWeight: 0,
      visible1: false,
      visible2: false,
      visible3: false,
      visible4: false,
      visibleBlockData: false,
      selectedBlocksString: localStorage.getItem("selectedBlocks"),
      icons: [],
      copied: false
    }
  },
  methods: {
    /**
     * Handles the drag over event.
     * @param {Event} event - The drag over event.
     */
    handleDragOver(event) {
      event.preventDefault();
      this.isDragging = true;
    },
    /**
     * Handles the drag leave event.
     * @param {Event} event - The drag leave event.
     */
    handleDragLeave(event) {
      event.preventDefault();
      this.isDragging = false;
    },
    /**
     * Handles the drop event.
     * @param {Event} event - The drop event.
     */
    handleDrop(event) {
      event.preventDefault();
      this.isDragging = false;
      const files = event.dataTransfer.files;
      this.handleFiles({ target: { files } });
    },
    /**
     * Handles the file input event.
     * @param {Event} event - The file input event.
     */
    handleFiles(event) {
      const files = event.target.files;
      this.parseFile(files[0]);
    },
    /**
     * Parses the file.
     * @param {File} file - The file to parse.
     */
    parseFile(file) {
      var reader = new FileReader();

      reader.onload = (e) => {
        try {
          this.importObject = JSON.parse(e.target.result)
          localStorage.setItem('importObject', JSON.stringify(this.importObject));
        } catch (error) {
          console.log(error)
          this.importObject = null
        }
      };
      reader.readAsText(file);
    },
    /**
     * Adds a block to the selection.
     * @param {Event} e - The event object.
     * @param {Object} block - The block to add.
     */
    addToSelection(e, block) {
      var amount = 1

      var indexLookup = this.selectedBlocks.findIndex((selectedBlock) => {
        return selectedBlock.uniqueID == block.uniqueID
      })

      if (indexLookup != -1) {
        this.selectedBlocks[indexLookup].amount += amount
      }
      else {
        var addedBlock = block
        addedBlock.amount = amount
        this.selectedBlocks.push(addedBlock)
      }

      this.selectedBlocks.sort((blockA, blockB) => {
        const nameA = blockA.displayName.toLowerCase(); // Case-insensitive sorting
        const nameB = blockB.displayName.toLowerCase();
        return nameA < nameB ? -1 : nameA > nameB ? 1 : 0;
      });
      this.calculateComponentList();
    },
    incrementBlockAmount(event, block, index) {
      block.amount++
      this.calculateComponentList();
    },
    /**
     * Decrements the amount of a block.
     * @param {Event} event - The event object.
     * @param {Object} block - The block to decrement.
     * @param {number} index - The index of the block in the selectedBlocks array.
     */
    decrementBlockAmount(event, block, index) {
      if (block.amount > 0) {
        block.amount--;
      } else {
        this.selectedBlocks.splice(index, 1)
      }
      this.calculateComponentList();
    },
    /**
     * Removes a block from the selection.
     * @param {Event} e - The event object.
     * @param {Object} block - The block to remove.
     * @param {number} index - The index of the block in the selectedBlocks array.
     */
    removeFromSelection(e, block, index) {
      var indexLookup = this.selectedBlocks.findIndex((selectedBlock) => {
        return selectedBlock.uniqueID == block.uniqueID
      })
      this.selectedBlocks.splice(indexLookup, 1)
      this.calculateComponentList();
    },
    /**
     * Calculates the component list based on the selected blocks.
     */
    calculateComponentList() {
      try {
        if (!this.importObject) {
          throw new Error("Import object is null or undefined");
        }
        if (!this.importObject.blocks) {
          throw new Error("Blocks array is missing in the import object");
        }
        if (!Array.isArray(this.importObject.blocks)) {
          throw new Error("Blocks is not an array");
        }
        if (!this.importObject.components) {
          throw new Error("Components array is missing in the import object");
        }
        if (!Array.isArray(this.importObject.components)) {
          throw new Error("Components is not an array");
        }
      } catch (error) {
        console.error("Error in import object:", error);
        // Handle the error here, e.g. show an error message to the user
      }

      this.componentList = []
      this.selectedBlocks.forEach(selectedBlock => {
        var defaultBlock = this.importObject.blocks.find((myBlock) => {
          return myBlock.uniqueID.match(selectedBlock.uniqueID)
        })
        var components = defaultBlock.components

        components.forEach(componentfromBlock => {
          var currentComponentID = componentfromBlock.componentID
          var currentComponentAmount = componentfromBlock.amount
          var indexLookup = this.componentList.findIndex((mycomponent) => {
            return mycomponent.componentID.match(currentComponentID)
          })

          if (indexLookup != -1) {
            this.componentList[indexLookup].amount += (currentComponentAmount * selectedBlock.amount)
            this.componentList[indexLookup].blocks.push(selectedBlock.displayName)
          }
          else {
            var addedComponents = { ...componentfromBlock };
            addedComponents.amount = currentComponentAmount * selectedBlock.amount
            addedComponents.icon = this.importObject.components.find(element => {
              return element.componentID.match(currentComponentID)
            }).icon
            addedComponents.blocks = [selectedBlock.displayName]

            /*console.log(this.importObject.components.find(element => {
              return element.componentID.match(currentComponentID)
            })
            )*/
            this.componentList.push(addedComponents)
          }

          this.componentList.sort((a, b) => {
            if (a.componentDisplayName < b.componentDisplayName) {
              return -1;
            }
            if (a.componentDisplayName > b.componentDisplayName) {
              return 1;
            }
            return 0;
          });

        });
      });
    },
    /**
     * Calculates the total volume and weight of the components.
     */
    calculateComponentValue() {
      this.totalVolume = 0
      this.totalWeight = 0
      this.componentList.forEach(component => {
        var componentLookup = this.importObject.components.find(element => {
          return element.componentID.match(component.componentID)
        })
        var componentVolume = componentLookup.volume
        var componentWeight = componentLookup.weight
        var componentAmount = component.amount
        this.totalVolume += componentVolume * componentAmount
        this.totalWeight += componentWeight * componentAmount
      });
    },
    /**
     * Clears the input and resets the app state.
     */
    clearInput() {
      this.totalVolume = 0
      this.totalWeight = 0
      this.componentList = []
      this.selectedBlocks = []
      this.importObject = null
      localStorage.setItem('importObject', JSON.stringify(this.importObject));
      localStorage.setItem('selectedBlocks', JSON.stringify(this.selectedBlocks));
    },
    /**
     * Generates the display string for a block's components.
     * @param {Object} block - The block object.
     * @returns {string} - The display string for the block's components.
     */
    componentDisplay(block) {
      var displayString = [];
      var index = 0;

      try {
        block.components.forEach(component => {
          displayString.push(component.displayName + ": " + (component.amount + (index <= block.isCritical ? " (functional)" : " (optional)")));
          index++;
        });

        displayString = displayString.join("\n");

        if (block.modContext != "()") {
          displayString += "\nMod: " + block.modContext.replace(".sbm", "");
        } else {
          displayString += "\nMod: Vanilla";
        }

      } catch (error) {
        console.error('Error generating component display:', error);
        return 'Error generating component display';
      }

      return displayString;
    },
    blockDisplay(component) {
      var displayString = [];
      var blocks = [...new Set(component.blocks)];

      try {
        blocks.forEach(block => {
          displayString.push(block);
        });

        displayString = displayString.join("\n");

      } catch (error) {
        console.error('Error generating component display:', error);
        return 'Error generating component display';
      }

      return displayString;
    },
    /**
     * Toggles the size filter for blocks.
     */
    toggleSizeFilter() {
      if (this.sizeFilter === "large/small") {
        this.sizeFilter = "large";
      } else if (this.sizeFilter === "large") {
        this.sizeFilter = "small";
      } else {
        this.sizeFilter = "large/small"; // Toggle back to empty string
      }
    },
    /**
     * Updates the selected blocks based on the selectedBlocksString.
     */
    updateSelectedBlocks() {

      // Convert both to JSON strings to compare their structure, not their reference
      const newSelectedBlocksString = this.selectedBlocksString
      const oldSelectedBlocksString = JSON.stringify(this.selectedBlocks);

      // Proceed only if there's an actual change in the selected blocks
      if (newSelectedBlocksString !== oldSelectedBlocksString) {
        try {
          this.selectedBlocks = JSON.parse(this.selectedBlocksString) || [];
        } catch (error) {
          console.error("Error parsing selected blocks:", error);
          // Handle the error here, e.g. show an error message to the user
          this.selectedBlocks = [];
        }
      }
      
    },
    /**
     * Updates the import object.
     * @param {string} value - The new import object value.
     */
    updateImportObject(value) {
      this.clearInput()

      try {
        this.importObject = JSON.parse(value)
        localStorage.setItem('importObject', JSON.stringify(this.importObject))
        this.importText = ""
      } catch (error) {
        console.log(error)
        this.importObject = null
        this.importText = ""
      }
    },
    /**
     * Looks up the icon for an object.
     * @param {Object} object - The object to lookup the icon for.
     * @returns {string} - The path to the icon image.
     */
    lookupIcon(object) {
      iconInput = object.icon;
      var indexLookup = this.icons.findIndex((iconName) => {
        return iconName == iconInput
      })
      if (indexLookup != -1) return 'assets/Icons/' + iconInput + '.png'
      else return 'assets/Icons/Symbol_X.png'
    },
    /**
     * Rounds a value to zero.
     * @param {number} value - The value to round.
     * @returns {number} - The rounded value.
     */
    roundToZero(value) {
      return Math.round(value);
    },
    copyToClipboard(input) {
      navigator.clipboard.writeText(input)
      this.copied = true
      setTimeout(() => {
        this.copied = false;
      }, 2000);
    }
  },
  computed: {
    /**
     * Filters the blocks based on the search string.
     * @returns {Array} - The filtered blocks.
     */
    filteredBlocks() {
      if (this.importObject.blocks != null) {
        return this.importObject.blocks
          .filter((block) => {
            const displayNameMatches = block.displayName.toUpperCase().includes(this.search.toUpperCase());
            const sizeMatches = this.sizeFilter === "large/small" || block.size.toUpperCase() === this.sizeFilter.toUpperCase();
            return displayNameMatches && sizeMatches;
          })
          .sort((a, b) => {
            if (a.displayName.toUpperCase() < b.displayName.toUpperCase()) {
              return -1;
            }
            if (a.displayName.toUpperCase() > b.displayName.toUpperCase()) {
              return 1;
            }
            return 0;
          });
      } else {
        return [];
      }
    },
    loadmasterOutput() {
      var outputstring = ""
      this.componentList.forEach(component => {
        outputstring += component.componentID.replace("MyObjectBuilder_","") + "=" + component.amount + "\n"
      });
      return outputstring;
    }
  },
  created() {
    this.$watch("selectedBlocks", (newSelectedBlocks, oldSelectedBlocks) => {
      
//        this.calculateComponentList();
        this.selectedBlocksString = JSON.stringify(this.selectedBlocks)
        localStorage.setItem('selectedBlocks', JSON.stringify(this.selectedBlocks));
    }, { deep: true });
  
    this.$watch("componentList", (newComponentList) => {
      this.calculateComponentValue();
    }, { deep: true });
  
    // Initial calculation to set up the component list based on the initially selected blocks
    this.calculateComponentList();
  },
  async mounted() {
    try {
      const response = await fetch('assets/Icons/icons.json');
      if (!response.ok) {
        throw new Error('Network response was not ok');
      }
      const data = await response.json();
      this.icons = data.icons;
    } catch (error) {
      console.error('Error fetching the icon assets:', error);
    }
  }
  // async mounted() {
  //   const response = await fetch('assets/Icons/');
  //   const text = await response.text();
  //   const regex = /assets\/Icons\/(\w+)\.png/gi;
  //   const fileNames = [];
  //   let match;
  //   while ((match = regex.exec(text)) !== null) {
  //     fileNames.push(match[1]);
  //   }
  //   this.icons = fileNames;
  // }
});

app.mount('#app')