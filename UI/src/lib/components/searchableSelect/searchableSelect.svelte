<style src="./searchableSelect.css"></style>

<script lang='ts'>
  export let options: string[];
  export let selectedItem: string | undefined;
  export let placeholder: string | undefined;
  export let onItemSelected: (item: string) => void;
  export let isEmptyByDefault: boolean = false;
  export let emptyPlaceholder: string | undefined = undefined;

  let searchText: string | undefined;
  let isOpen: boolean = false;

  let popupSearchBox: HTMLElement | undefined;
</script>

<div class='searchable-select'>
  <button on:click={() => {
    isOpen = !isOpen;
    setTimeout(() => {
      popupSearchBox?.focus();
    }, 1);
  }} class={'inline-button ' + (isEmptyByDefault && !selectedItem ? 'empty' : '')}>{(selectedItem || emptyPlaceholder) ?? options?.[0]}
  </button>
  <div class='popout-options' style={isOpen? 'display: block' : ''}>
    <input bind:this={popupSearchBox} type='text' bind:value={searchText} placeholder={placeholder}/>
    <div class='scrollable-container'>
      <ul>
        {#if isEmptyByDefault}
          <!-- svelte-ignore a11y-click-events-have-key-events -->
          <!-- svelte-ignore a11y-no-noninteractive-element-interactions -->
          <li on:click={() => {
            onItemSelected('');
            isOpen = false;
            searchText = '';
          }} style='height: 1.5rem'></li>
        {/if}
        {#each options.filter(x => !searchText || x.includes(searchText)) as option}
          <!-- svelte-ignore a11y-click-events-have-key-events -->
          <!-- svelte-ignore a11y-no-noninteractive-element-interactions -->
          <li on:click={() => {
            onItemSelected(option);
            isOpen = false;
            searchText = '';
          }}>{option}</li>
        {/each}
      </ul>
    </div>
  </div>
</div>